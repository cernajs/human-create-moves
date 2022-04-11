using System;
using System.Collections.Generic;

namespace movesCreate
{
    class Program
    {
        private char[][] grid;
        private Position boxPosition;
        private int[] directions = new int[] { 0, 1, 0, -1, 0 };

        public struct Position
        {
            public readonly int X;
            public readonly int Y;
            public readonly int LENGTH;

            public Position(int x, int y, int length)
            {
                X = x;
                Y = y;
                LENGTH = length;
            }

        }

        public struct BoxAndPlayerPosition
        {
            public Position boxPosition;
            public Position playerPosition;

            public BoxAndPlayerPosition(Position boxPosition, Position playerPosition)
            {
                this.boxPosition = boxPosition;
                this.playerPosition = playerPosition;
            }

            
            public BoxAndPlayerPosition NextPosition()
            //kam se pohneme dál při posunu krabice
            {
                if (boxPosition.X == playerPosition.X)
                {
                    if (playerPosition.Y < boxPosition.Y)
                        return new BoxAndPlayerPosition(new Position(boxPosition.X, boxPosition.Y + 1, 0),
                            new Position(playerPosition.X, playerPosition.Y + 1, playerPosition.LENGTH + 1));
                    return new BoxAndPlayerPosition(new Position(boxPosition.X, boxPosition.Y - 1, 0),
                        new Position(playerPosition.X, playerPosition.Y - 1, playerPosition.LENGTH + 1));
                }

                if (playerPosition.X < boxPosition.X)
                    return new BoxAndPlayerPosition(new Position(boxPosition.X + 1, boxPosition.Y, 0),
                        new Position(playerPosition.X + 1, playerPosition.Y, playerPosition.LENGTH + 1));
                return new BoxAndPlayerPosition(new Position(boxPosition.X - 1, boxPosition.Y, 0),
                    new Position(playerPosition.X - 1, playerPosition.Y, playerPosition.LENGTH + 1));
            }
        }

        private int PlayerCanMove(BoxAndPlayerPosition current, Position target)
        {
            //zjistí jestli se můžeme pohnout z current do targetu.

            Queue<Position> queue = new Queue<Position>();
            boxPosition = current.boxPosition;

            queue.Enqueue(current.playerPosition);
            HashSet<string> visited = new HashSet<string>();
            var vs = current.playerPosition.X.ToString() + current.playerPosition.Y.ToString();
           
            visited.Add(vs);
            
            //BFS
            while (queue.Count > 0)
            {
                Position currPos = queue.Dequeue();
               
                int cur_length = currPos.LENGTH;
                if (AreEqualPositions(currPos,target))
                {
                    //jsme v targetu
                    return currPos.LENGTH;
                }

                for (int i = 0; i < 4; i++)
                {
                    //projedeme 4 směry kame se můžeme pohnout
                    int newX = currPos.X + directions[i];
                    int newY = currPos.Y + directions[i + 1];
                    Position newPos = new Position(newX, newY, cur_length + 1);

                    if (CanMoveTo(newPos, visited))
                    {
                        visited.Add(newPos.X.ToString() + newPos.Y.ToString());
                        queue.Enqueue(newPos);
                    }
                }
            }
            
            return -1;
        }

        private bool CanMoveTo(Position pos, HashSet<string> visited)
        {
            return (IsValidAndEmpty(pos) && !visited.Contains(pos.X.ToString() + pos.Y.ToString()) &&
                    !AreEqualPositions(pos, boxPosition));
        }

        private bool AreEqualPositions(Position pos1, Position pos2)
        {
            return (pos1.X.ToString() + pos1.Y.ToString() == pos2.X.ToString() + pos2.Y.ToString());
        }


        private bool IsValidAndEmpty(Position pos)
        {
            int x = pos.X;
            int y = pos.Y;

            return x >= 0 && x < grid[0].Length && y >= 0 && y < grid.Length && grid[y][x] == '.';
        }

        public int MinPushBox(char[][] grid)
        {
            this.grid = grid;

            Position boxPosition = FindBox(grid);
            Position playerPosition = FindPlayer(grid);
            Position targetPosition = FindTarget(grid);

            // změníme hodnoty na místech boxu, hráče a targetu, abychom se skrz ně mohli pohnout
            grid[boxPosition.Y][boxPosition.X] = '.';
            grid[playerPosition.Y][playerPosition.X] = '.';
            grid[targetPosition.Y][targetPosition.X] = '.';


            Queue<BoxAndPlayerPosition> queue = new Queue<BoxAndPlayerPosition>();
            HashSet<string> visited = new HashSet<string>();

            BoxAndPlayerPosition initial = new BoxAndPlayerPosition(boxPosition, playerPosition);


            // přidáme všechny možnosti : box a hráč - vleve,vpravo,nahoře,dole
            for (int i = 0; i < 4; i++)
            {

                int newX = boxPosition.X + directions[i];
                int newY = boxPosition.Y + directions[i + 1];
                int first_len = PlayerCanMove(initial, new Position(newX, newY,0)); //vzdálenost z původního místa skladníka k boxu
                Position target = new Position(newX, newY, first_len);


                if (!IsValidAndEmpty(target) || PlayerCanMove(initial, target) == -1)
                    continue;

                var start = new BoxAndPlayerPosition(boxPosition, target);
                queue.Enqueue(start);
                visited.Add(start.playerPosition.X.ToString() + start.playerPosition.Y.ToString() + start.boxPosition.X.ToString() + start.boxPosition.Y.ToString());
            }

           //BFS
            while (queue.Count > 0)
            {
                var length = queue.Count;

                for (int j = 0; j < length; j++)
                {
                    BoxAndPlayerPosition current = queue.Dequeue();
                    
                    if (AreEqualPositions(current.boxPosition, targetPosition))
                        //kontrola jestli jsme v cíli
                        return current.playerPosition.LENGTH;

                    var next = current.NextPosition();
                    var nextBox = next.boxPosition;

                    if (!visited.Contains(next.playerPosition.X.ToString() + next.playerPosition.Y.ToString() + next.boxPosition.X.ToString() + next.boxPosition.Y.ToString()) && IsValidAndEmpty(nextBox))
                    {
                        //next(pozici při púosunutí krabice) jsme ještě nenavštívili
                        queue.Enqueue(next);
                        visited.Add(next.playerPosition.X.ToString() + next.playerPosition.Y.ToString() + next.boxPosition.X.ToString() + next.boxPosition.Y.ToString());
                        
                        for (int i = 0; i < 4; i++)
                        {
                            //pro každou pozici boxu přidáme 4 pozice boxu
                            
                            int newX = nextBox.X + directions[i];
                            int newY = nextBox.Y + directions[i + 1];
                            Position newPosition = new Position(newX, newY,current.playerPosition.LENGTH+ PlayerCanMove(current, new Position(newX, newY, 0)));
                            //nová pozice, ke vzdálenosti musíme připočíst BFS mezi current a newX, newY -> můžou zde být překážky


                            if (!next.playerPosition.Equals(newPosition) && IsValidAndEmpty(newPosition) &&
                                PlayerCanMove(next, newPosition) != -1)
                            {
                                
                                var start = new BoxAndPlayerPosition(nextBox, newPosition);
                                if (!visited.Contains(start.playerPosition.X.ToString() + start.playerPosition.Y.ToString() + start.boxPosition.X.ToString() + start.boxPosition.Y.ToString()))
                                {
                                    queue.Enqueue(start);
                                    visited.Add(start.playerPosition.X.ToString() + start.playerPosition.Y.ToString() + start.boxPosition.X.ToString() + start.boxPosition.Y.ToString());
                                }
                            }
                        }
                    }
                }

            }
            //cesta neexistuje
            return -1;
        }

        private Position FindBox(char[][] grid)
        {
            return Find(grid, 'B');
        }

        private Position FindPlayer(char[][] grid)
        {
            return Find(grid, 'S');
        }

        private Position FindTarget(char[][] grid)
        {
            return Find(grid, 'C');
        }

        private Position Find(char[][] grid, char c)
        {
            for (int i = 0; i < grid.Length; i++)
                for (int j = 0; j < grid[0].Length; j++)
                    if (grid[i][j] == c)
                        return new Position(j, i,0);

            throw new Exception();
        }

        static void Main(string[] args)
        {

            char[][] grid = new char[][]
            {
                new char[] {'#', '#', '#', '#', '#', '#'},
                new char[] {'#', 'C', '#', '#', '#', '#'},
                new char[] {'#', '.', 'B', '.', '.', '#'},
                new char[] {'#', '.', '#', '#', '.', '#'},
                new char[] {'#', '.', '.', '.', 'S', '#'},
                new char[] {'#', '#', '#', '#', '#', '#'},
            };
            //Console.WriteLine(MinPushBox(grid));
            Program program = new Program();
            Console.WriteLine(program.MinPushBox(grid));
        }
    }
}
