using System;
using System.Collections.Generic;

namespace movesCreate
{
    class Program
    {
        private char[][] grid;
        private Position boxPosition;
        private int[] directions = new int[] {0, 1, 0, -1, 0};

        public struct Position
        {
            public int X;
            public int Y;

            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override string ToString()
            {
                return $"[{Y},{X}]";
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

            // where the box will be moved along with player. Player will fill in the box spot, box will be moved 1 cell in corresponding direction
            public BoxAndPlayerPosition NextPosition()
            {
                if (boxPosition.X == playerPosition.X)
                {
                    if (playerPosition.Y < boxPosition.Y)
                        return new BoxAndPlayerPosition(new Position(boxPosition.X, boxPosition.Y + 1),
                            new Position(playerPosition.X, playerPosition.Y + 1));
                    return new BoxAndPlayerPosition(new Position(boxPosition.X, boxPosition.Y - 1),
                        new Position(playerPosition.X, playerPosition.Y - 1));
                }

                if (playerPosition.X < boxPosition.X)
                    return new BoxAndPlayerPosition(new Position(boxPosition.X + 1, boxPosition.Y),
                        new Position(playerPosition.X + 1, playerPosition.Y));
                return new BoxAndPlayerPosition(new Position(boxPosition.X - 1, boxPosition.Y),
                    new Position(playerPosition.X - 1, playerPosition.Y));
            }

            public override string ToString()
            {
                return $"b:{boxPosition};p:{playerPosition}";
            }
        }

        private bool PlayerCanMove(BoxAndPlayerPosition current, Position target)
        {
            Queue<Position> queue = new Queue<Position>();
            boxPosition = current.boxPosition;

            queue.Enqueue(current.playerPosition);
            HashSet<Position> visited = new HashSet<Position>();
            visited.Add(current.playerPosition);

            while (queue.Count > 0)
            {
                Position currPos = queue.Dequeue();
                if (currPos.Equals(target))
                {
                    return true;
                }

                for (int i = 0; i < 4; i++)
                {
                    int newX = currPos.X + directions[i];
                    int newY = currPos.Y + directions[i + 1];
                    Position newPos = new Position(newX, newY);

                    if (CanMoveTo(newPos, visited))
                    {
                        visited.Add(newPos);
                        queue.Enqueue(newPos);
                    }
                }
            }

            return false;
        }

        private bool CanMoveTo(Position pos, HashSet<Position> visited)
        {
            return (IsValidAndEmpty(pos) && !visited.Contains(pos) &&
                    !pos.Equals(boxPosition));
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

            // since we captured box, player, and target positions, we change it to '.' so it would be easier to check whether we can move in a cell
            grid[boxPosition.Y][boxPosition.X] = '.';
            grid[playerPosition.Y][playerPosition.X] = '.';
            grid[targetPosition.Y][targetPosition.X] = '.';

            int pushes = 0;
            Queue<BoxAndPlayerPosition> queue = new Queue<BoxAndPlayerPosition>();
            HashSet<BoxAndPlayerPosition> visited = new HashSet<BoxAndPlayerPosition>();

            BoxAndPlayerPosition initial = new BoxAndPlayerPosition(boxPosition, playerPosition);

            // adding all possible starting points: box and positions to the left, right, top, bottom to the box.
            for (int i = 0; i < 4; i++)
            {
                int newX = boxPosition.X + directions[i];
                int newY = boxPosition.Y + directions[i + 1];
                Position target = new Position(newX, newY);

                if (!IsValidAndEmpty(target) || !PlayerCanMove(initial, target))
                    continue;

                var start = new BoxAndPlayerPosition(boxPosition, target);
                queue.Enqueue(start);
                visited.Add(start);
            }

            // BFS for player and the box
            while (queue.Count > 0)
            {
                var length = queue.Count;

                for (int j = 0; j < length; j++)
                {
                    BoxAndPlayerPosition current = queue.Dequeue();
                    if (current.boxPosition.Equals(targetPosition))
                        return pushes;

                    var next = current.NextPosition();
                    var nextBox = next.boxPosition;

                    if (!visited.Contains(next) && IsValidAndEmpty(nextBox))
                    {
                        queue.Enqueue(next);
                        visited.Add(next);

                        // when we add another position 'next' for the player and the box, we need to try to add all possible positions for the player if the box is fixed. So whenever we move the box, always add all positions for the player as well around the box to the queue.
                        for (int i = 0; i < 4; i++)
                        {
                            int newX = nextBox.X + directions[i];
                            int newY = nextBox.Y + directions[i + 1];
                            Position newPosition = new Position(newX, newY);

                            if (!next.playerPosition.Equals(newPosition) && IsValidAndEmpty(newPosition) &&
                                PlayerCanMove(next, newPosition))
                            {
                                var start = new BoxAndPlayerPosition(nextBox, newPosition);
                                if (!visited.Contains(start))
                                {
                                    queue.Enqueue(start);
                                    visited.Add(start);
                                }
                            }
                        }
                    }
                }

                pushes++;
            }

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
            return Find(grid, 'T');
        }

        private Position Find(char[][] grid, char c)
        {
            for (int i = 0; i < grid.Length; i++)
            for (int j = 0; j < grid[0].Length; j++)
                if (grid[i][j] == c)
                    return new Position(j, i);

            throw new Exception();
        }

        void Main(string[] args)
        {

            char[][] grid = new char[][]
            {
                new char[] {'#', '#', '#', '#', '#', '#'},
                new char[] {'#', 'T', '#', '#', '#', '#'},
                new char[] {'#', '.', '.', 'B', '.', '#'},
                new char[] {'#', '.', '#', '#', '.', '#'},
                new char[] {'#', '.', '.', '.', 'S', '#'},
                new char[] {'#', '#', '#', '#', '#', '#'},
            };
            Console.WriteLine(MinPushBox(grid));
        }
    }
}