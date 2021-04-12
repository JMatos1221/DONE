using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using System.Threading;

public class Done : AbstractThinker
{
    private const float win = float.PositiveInfinity;
    private const float loss = float.NegativeInfinity;
    private int maxDepth;

    public override string ToString()
    {
        return base.ToString() + "_V2";
    }

    public override void Setup(string str)
    {
        maxDepth = 2;
    }

    public override FutureMove Think(Board board, CancellationToken ct)
    {
        (FutureMove move, float score) decision = Negamax(board, ct, board.Turn, 0);

        return decision.move;
    }

    private (FutureMove move, float score) Negamax(Board board, CancellationToken ct, PColor player, int depth)
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

                    float eval = -Negamax(board, ct, player.Other(), depth + 1).score;

                    board.UndoMove();

                    if (eval > bestMove.score)
                        bestMove = (new FutureMove(i, shape), eval);
                }
            }
        }
        return bestMove;
    }

    private float Heuristic(Board board, PColor player)
    {
        float PieceChain(Piece piece, int i, int j)
        {
            float chainValue = 0;

            if (piece.color == player)
            {

            }

            return chainValue;
        }

        float val = 0;

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                Piece? piece = board[i, j];

                if (piece.HasValue)
                {

                }
            }
        }
        return val;
    }
}