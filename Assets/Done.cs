using ColorShapeLinks.Common;
using ColorShapeLinks.Common.AI;
using System.Threading;

public class Done : AbstractThinker
{
    private const int win = 1000;
    private const int loss = -1000;
    private int maxDepth;

    public override string ToString()
    {
        return base.ToString() + "_V1";
    }

    public override void Setup(string str)
    {
        maxDepth = 50;
    }

    public override FutureMove Think(Board board, CancellationToken ct)
    {
        (FutureMove move, int score) decision = Negamax(board, ct, board.Turn, 0);

        return decision.move;
    }

    private (FutureMove move, int score) Negamax(Board board, CancellationToken ct, PColor player, int depth)
    {
        if (board.CheckWinner().ToPColor() == player)
        {
            return (FutureMove.NoMove, win);
        }

        else if (board.CheckWinner().ToPColor() == player.Other())
        {
            return (FutureMove.NoMove, loss);
        }

        else if (board.CheckWinner() == Winner.Draw)
        {
            return (FutureMove.NoMove, 0);
        }

        else if (depth == maxDepth)
        {
            return (FutureMove.NoMove, 0);
        }

        else
        {
            (FutureMove move, int score) bestMove = (new FutureMove(0, PShape.Round), loss);

            for (int i = 0; i < Cols; i++)
            {
                if (!board.IsColumnFull(i))
                    continue;

                for (int j = 0; j < 2; j++)
                {
                    PShape shape = (PShape)j;

                    if (board.PieceCount(board.Turn, shape) == 0) continue;

                    board.DoMove((PShape)j, i);

                    int score = -Negamax(board, ct, board.Turn, depth + 1).score;

                    if (score > bestMove.score)
                        bestMove = (new FutureMove(i, (PShape)j), score);

                    board.UndoMove();
                }
            }
            return bestMove;
        }
    }
}
