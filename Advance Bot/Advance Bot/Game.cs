using Advance_Bot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Advance_Bot
{
    internal class Game
    {
        char[,] board;
        int boardSize = 9;
        private List<(char Piece, int X, int Y)> openingMoves;
        private int openingMoveIndex;

        public Game()
        {
            openingMoves = new List<(char Piece, int Y, int X)>
               {
                    ('Z', 2, 1),
                    ('Z', 2, 2),
                    ('Z', 2, 3),
                    ('Z', 2, 4),
                    ('Z', 2, 5),
                    ('Z', 2, 6),
                    ('Z', 2, 7),
                    ('S', 2, 3),
                    ('M', 2, 4),
                    ('B', 2, 5),
                    ('D', 1, 4),
                    ('G', 1, 4),
                    ('J', 1, 3),
               };
            openingMoveIndex = 0;
        }

        private void MakeOpeningMove()
        {
            if (openingMoveIndex >= openingMoves.Count)
            {
                // All openers have been made
                return;
            }

            var move = openingMoves[openingMoveIndex];
            openingMoveIndex += 1;

            // Check if a 'Z' piece can be moved down
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (board[i, j] == move.Piece && board[i + 1, j] == '.')
                    {
                        // Found piece, move to position
                        board[i + 1, j] = move.Piece;
                        // Clear old pos
                        board[i, j] = '.';
                        return;  // Exit once a piece has been moved
                    }
                }
            }
        }


        public void BotMakesMove()
        {
            MakeOpeningMove();
            PrintBoard();
        }


        public void LoadBoard(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            board = new char[boardSize, boardSize];

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    board[i, j] = lines[i][j];
                }
            }
        }


        public void PrintBoard()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    Console.Write(board[i, j]);
                }
                Console.WriteLine();
            }
        }


        public void ApplyPlayerMove(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            char[,] newBoard = new char[boardSize, boardSize];

            // Create instance of Rules class
            moveValidity rules = new moveValidity();

            int oldX = -1, oldY = -1, newX = -1, newY = -1;
            char movedPiece = '.';

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    newBoard[i, j] = lines[i][j];

                    if (board[i, j] != lines[i][j])
                    {
                        if (Char.IsLower(board[i, j]))
                        {
                            // A piece has moved from this square
                            movedPiece = board[i, j];
                            oldX = i;
                            oldY = j;
                        }
                        else if (Char.IsLower(lines[i][j]))
                        {
                            // A piece has moved to this square
                            newX = i;
                            newY = j;
                        }
                    }
                }
            }

            if (movedPiece != '.' && oldX != -1 && oldY != -1 && newX != -1 && newY != -1)
            {
                if (rules.IsMoveValid(movedPiece, oldX, oldY, newX, newY))
                {
                    // Update the board state before printing it.
                    board = newBoard;

                    Console.WriteLine($"Player's move: piece {movedPiece} moved from ({oldX},{oldY}) to ({newX},{newY}).\n");
                    PrintBoard();
                }
                else
                {
                    Console.WriteLine("move is invalid!");
                }
            }
            else
            {
                // If no valid move was detected, update the board state.
                board = newBoard;
            }
        }


        public void WaitForPlayerMove(string expectedFileName)
        {
            string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
            string expectedFilePath = Path.Combine(directoryPath, expectedFileName);

            Console.WriteLine("Waiting for player's move...");

            while (!File.Exists(expectedFilePath))
            {
                // Just sleeping the program until the players move can be found in the directory
                Thread.Sleep(100);
            }

            if (File.Exists(expectedFilePath))
            {
                ApplyPlayerMove(expectedFilePath);
                return;
            }

            var fileWatcher = new FileSystemWatcher
            {
                Path = directoryPath,
                Filter = expectedFileName,
                NotifyFilter = NotifyFilters.FileName,
            };

            AutoResetEvent fileCreatedEvent = new AutoResetEvent(false);
            fileWatcher.Created += (sender, args) =>
            {
                fileCreatedEvent.Set();
                ApplyPlayerMove(expectedFilePath);
            };

            fileWatcher.EnableRaisingEvents = true;

            fileCreatedEvent.WaitOne();
        }


        public List<Move> GetPossibleMoves(int startX, int startY)
        {
            var possibleMoves = new List<Move>();


            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    int newX = startX + dx;
                    int newY = startY + dy;

                    if (newX >= 0 && newX < boardSize && newY >= 0 && newY < boardSize)
                    {
                        moveValidity rules = new moveValidity();
                        char piece = board[startX, startY];

                        if (rules.IsMoveValid(piece, startX, startY, newX, newY))
                        {
                            possibleMoves.Add(new Move(startX, startY, newX, newY));
                        }
                    }
                }
            }

            return possibleMoves;
        }


        public int ScoreMove(Move move, char[,] board)
        {
            int botBoardScore = 48;
            int playerBoardScore = 48;

            Dictionary<char, int> pieceScores = new Dictionary<char, int>() 
            {
                {'z', 1},
                {'b', 2},
                {'j', 3},
                {'m', 4},
                {'s', 5},
                {'c', 6},
                {'d', 7},
            };

            char targetPiece = board[move.ToX, move.ToY];
            int score;

            // check if the move would result in a capture
            if (Char.IsLower(targetPiece))
            {
                // decrease player score
                playerBoardScore -= pieceScores[targetPiece];
                // increase bot score
                botBoardScore += pieceScores[targetPiece];
                score = botBoardScore - playerBoardScore;
            }
            else if (targetPiece == '.')
            {
                // no change in board scores, neutral move
                score = 0;
            }
            else
            {
                // we're losing a piece, decrease bot score
                botBoardScore -= pieceScores[char.ToLowerInvariant(targetPiece)];
                score = botBoardScore - playerBoardScore;
            }

            return score;
        }


        public List<(char Piece, int X, int Y)> GetBotPieces(char[,] board)
        {
            var botPieces = new List<(char Piece, int X, int Y)>();

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (Char.IsUpper(board[i, j])) 
                    {
                        botPieces.Add((board[i, j], i, j));
                    }
                }
            }

            return botPieces;
        }


        public void ApplyMove(Move move, char[,] board) 
        { 
            char piece = board[move.FromX, move.FromY];
            board[move.FromX, move.FromY] = '.';
            board[move.ToX, move.ToY] = piece;
        }


        public Move makeBotsBestMove() 
        {
            int maxScore = int.MinValue;
            Move bestMove = null;
            var botPieces = GetBotPieces(board);

            foreach (var botPiece in botPieces)
            {
                var moves = GetPossibleMoves(botPiece.X, botPiece.Y);

                foreach (var move in moves) 
                {
                    int moveScore = ScoreMove(move, board);

                    if (moveScore > maxScore) 
                    { 
                        maxScore = moveScore;
                        bestMove = move;
                    }
                }
            }

            if (bestMove != null)
            {
                ApplyMove(bestMove, board);
            }

            return bestMove;
        }


        public void GameLoop()
        {
            Console.WriteLine("Welcome to Advance!\n");
            LoadBoard("init_board.txt");
            PrintBoard();

            int playerMoveCounter = 1;
            while (true)
            {
                WaitForPlayerMove($"{playerMoveCounter}.txt");
                Console.WriteLine("Bot's move:\n");
                BotMakesMove();
                playerMoveCounter++;
            }
        }

    }
}
