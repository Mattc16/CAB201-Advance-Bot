using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advance_Bot
{
    internal class moveValidity
    {
        public bool IsMoveValid(char piece, int oldX, int oldY, int newX, int newY)
        {

            int dy = newX - oldX;
            int dx = newY - oldY;
            Console.WriteLine($"Checking move validity for piece {piece} from ({oldX},{oldY}) to ({newX},{newY}).");
            Console.WriteLine($"change in x = {dx}, change in y = {dy}");


            switch (char.ToLower(piece))
            {
                case 'z': // Zombie
                    if ((dx == -1 || dx == 1 || dx == 0) && dy == -1)
                    {
                        return true;
                    }
                    break;

                case 'b': // Builder
                    if (dx <= 1 && dy <= 1) // Any of the 8 adjoining squares
                        return true;
                    break;

                case 'j': // Jester
                    if (dx <= 1 && dy <= 1) // Any of the 8 adjoining squares
                        return true;
                    break;

                case 'm': // Miner
                    if (dx == 0 || dy == 0) // Any number of squares in one of the 4 cardinal directions
                        return true;
                    break;

                case 's': // Sentinel
                    if ((dx == 2 && dy == 1) || // Two squares in one direction and one in perpendicular direction
                        (dx == 1 && dy == 2))
                        return true;
                    break;

                case 'c': // Catapult
                    if ((dx == 0 && dy == 1) || // One square in any of the 4 cardinal directions
                        (dy == 0 && dx == 1))
                        return true;
                    break;

                case 'd': // Dragon
                    if ((dx == 0 || dy == 0 || dx == Math.Abs(dy)) && // Diagonal or cardinal movement
                        (Math.Abs(dx) > 1 || Math.Abs(dy) > 1)) // Cannot capture adjacent pieces
                        return true;
                    break;

                case 'g': // General
                    if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1) // Any of the 8 adjoining squares
                        return true;
                    break;

                default:
                    return false;
            }
            return false;
        }


    }
}
