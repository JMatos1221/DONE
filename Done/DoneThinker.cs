using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using System.Threading;
using System;

namespace Done
{
    public class DoneThinker : AbstractThinker
    {
        private const float win = float.PositiveInfinity;
        private const float loss = float.NegativeInfinity;
        private int maxDepth;

        public override string ToString()
        {
            return "G10_" + base.ToString() + "_V6";
        }

        public override void Setup(string str)
        {
            maxDepth = 3;
        }

        public override FutureMove Think(Board board, CancellationToken ct)
        {
            (FutureMove move, float score) decision = Negamax(board, ct, board.Turn, 0, float.NegativeInfinity, float.PositiveInfinity);

            return decision.move;
        }

        private (FutureMove move, float score) Negamax(Board board, CancellationToken ct, PColor player, int depth, float alpha, float beta)
        {
            (FutureMove move, float score) bestMove;
            Winner winner;

            // If a cancellation request was made...
            if (ct.IsCancellationRequested)
            {
                // ...set a "no move" and skip the remaining part of
                // the algorithm
                bestMove = (FutureMove.NoMove, float.NaN);
            }

            else if ((winner = board.CheckWinner()) != Winner.None)
            {
                PColor winnerColor = winner.ToPColor();

                if (winnerColor == player)
                {
                    bestMove = (FutureMove.NoMove, win);
                }

                else if (winnerColor == player.Other())
                {
                    bestMove = (FutureMove.NoMove, loss);
                }

                else
                {
                    bestMove = (FutureMove.NoMove, 0f);
                }
            }

            else if (depth == maxDepth)
            {
                bestMove = (FutureMove.NoMove, Heuristic(board, player));
            }

            else
            {
                bestMove = (FutureMove.NoMove, loss);

                for (int i = 0; i < Cols; i++)
                {
                    if (board.IsColumnFull(i))
                        continue;

                    for (int j = 0; j < 2; j++)
                    {
                        PShape shape = (PShape)j;

                        if (board.PieceCount(player, shape) == 0) continue;

                        board.DoMove(shape, i);

                        float eval = -Negamax(board, ct, player.Other(), depth + 1, float.NegativeInfinity, float.PositiveInfinity).score;

                        board.UndoMove();

                        if (eval > bestMove.score)
                            bestMove = (new FutureMove(i, shape), eval);

                        if (eval > alpha) alpha = eval;

                        if (beta < alpha) return bestMove;
                    }
                }
            }
            return bestMove;
        }

        private float Heuristic(Board board, PColor player)
        {
            float PieceChain(Piece? piece, int x, int y)
            {
                PColor pieceColor = piece.Value.color;
                PShape pieceShape = piece.Value.shape;

                float chainValue = board.cols / 2 - Math.Abs(board.cols / 2 - y) * 8;

                for (int i = -1; i < 2; i++)
                    for (int j = -1; j < 2; j++)
                    {
                        if (i == 0 && j == 0) continue;

                        if (x + i < 0 || x + i >= board.rows ||
                            y + j < 0 || y + j >= board.cols) continue;

                        Piece? currentPiece = board[x + i, y + j];

                        if (currentPiece.HasValue)
                        {
                            if (currentPiece.Value.color == pieceColor)
                            {
                                chainValue += 4;

                                if (currentPiece.Value.shape == pieceShape &&
                                    pieceShape == player.Other().Shape())
                                    chainValue -= 2;

                                else if (currentPiece.Value.shape == pieceShape &&
                                    pieceShape == player.Shape())
                                    chainValue -= 1;
                            }

                            else if (currentPiece.Value.shape == pieceShape)
                            {
                                chainValue += 6;
                            }
                        }
                    }

                return chainValue;
            }

            float val = 0;

            for (int i = 0; i < board.rows; i++)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    Piece? piece = board[i, j];

                    if (piece.HasValue && piece.Value.color == player)
                    {
                        val += PieceChain(piece, i, j);
                    }

                    else if (piece.HasValue && piece.Value.color == player.Other())
                    {
                        val -= PieceChain(piece, i, j);
                    }
                }
            }
            return val;
        }
    }
}