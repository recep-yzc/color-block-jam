using System;
using System.Threading;
using System.Threading.Tasks;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow
    {
        private void StartValidation()
        {
            if (validation != null)
            {
                return;
            }

            StopPreview();
            var board = BoardFactory.Create(level.ToData());
            validationRevision = revision;
            validation = Task.Run(() => new BoardSolver().Solve(board, ValidationBudget));
            isLayoutStale = true;
        }

        private void SetPreviewStep(int step)
        {
            if (result == null || !result.IsSolved)
            {
                return;
            }

            previewStep = Mathf.Clamp(step, 0, result.Moves.Count);
            isLayoutStale = true;
            if (previewStep == 0)
            {
                preview = null;
                Repaint();
                return;
            }

            preview = BoardFactory.Create(level.ToData());
            for (var i = 0; i < previewStep; i++)
            {
                var move = result.Moves[i];
                var block = preview.Blocks[move.BlockId];
                if (move.Exits)
                {
                    preview.Clear(block);
                }
                else
                {
                    preview.Move(block, move.Target);
                }
            }

            Repaint();
        }

        private void StopPreview()
        {
            preview = null;
            previewStep = 0;
        }

        private void Generate()
        {
            var generator = new LevelGenerator();
            var settings = GeneratorSettings.For(generateDifficulty);
            settings.Holes = generateHoles;
            settings.MaxHoleSize = HoleSizeLimit;
            GeneratedLevel generated;
            using var cancellation = new CancellationTokenSource();
            try
            {
                generated = generator.Generate(settings, palette.Count, generateSeed, cancellation.Token, attempt =>
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Level Editor", $"Looking for a {generateDifficulty} level… try {attempt}",
                            attempt / (float)LevelGenerator.MaxAttempts))
                    {
                        cancellation.Cancel();
                    }
                });
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (generated == null)
            {
                EditorUtility.DisplayDialog("Level Editor", "No level was found with this seed. Try another seed.", "OK");
                return;
            }

            RecordUndo();
            SetLevel(EditableLevel.From(generated.Level), dirty: true);
            result = generated.Solution;
            resultRevision = revision;
            generateSeed++;
        }

        private LevelDifficulty SuggestDifficulty(SolveResult solution)
        {
            return LevelRating.Rate(LevelRating.MovesToWin(level.Blocks.Count, solution));
        }

        private string Describe(LevelProblem problem)
        {
            var colorName = problem.Color >= 0 && problem.Color < palette.Count ? palette.GetName(problem.Color) : "?";
            return problem.Kind switch
            {
                LevelProblemKind.NoBlocks => "The board has no blocks.",
                LevelProblemKind.BlockOutsideBoard => $"A {colorName} block is outside the board.",
                LevelProblemKind.BlocksOverlap => $"A {colorName} block overlaps another block.",
                LevelProblemKind.BlockOnHole => $"A {colorName} block sits on a removed cell.",
                LevelProblemKind.HoleTooSmall => $"Holes must be at least {LevelDiagnostics.MinHoleSize}×{LevelDiagnostics.MinHoleSize} cells.",
                LevelProblemKind.DoorOutsideBoard => $"A {colorName} door is off the edge.",
                LevelProblemKind.DoorsOverlap => $"Two doors overlap ({colorName}).",
                LevelProblemKind.DoorFacesHole => $"A {colorName} door opens onto a removed cell.",
                LevelProblemKind.ColorHasNoDoor => $"{colorName} blocks have no {colorName} door to leave through.",
                LevelProblemKind.BlockFitsNoDoor => $"A {colorName} block fits no {colorName} door it can reach. An arrow " +
                                                    "block reaches only the doors ahead of it along its arrow.",
                LevelProblemKind.IceNeverMelts => $"A {colorName} block has more ice than there are other blocks to melt it.",
                LevelProblemKind.EmptyBlock => $"A {colorName} block has no cells.",
                _ => problem.Kind.ToString()
            };
        }
    }
}
