using UnityEngine;

namespace TetrisZ
{
    public class Piece : MonoBehaviour
    {
        public Board board { get; private set; }
        public TetrominoData data { get; private set; }
        public Vector3Int position { get; private set; } // spawn position
        public Vector3Int[] cells { get; private set; }
        public int rotationIndex { get; private set; }

        public float stepDelay = 1f;
        public float lockDelay = 0.5f;

        private float stepTime;
        private float lockTime;

        public void Initialize(Board board, Vector3Int position, TetrominoData data)
        {
            this.board = board;
            this.position = position;
            this.data = data;
            this.rotationIndex = 0;
            rotationIndex = 0;
            stepTime = Time.time + stepDelay;
            lockTime = 0f;
            if (this.cells == null)
            {
                this.cells = new Vector3Int[data.cells.Length];
            }
            for (int i = 0; i < data.cells.Length; i++)
            {
                this.cells[i] = (Vector3Int)data.cells[i];
            }
        }

        private void Update()
        {
            this.board.Clear(this);

            lockTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Rotate(-1);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Rotate(1);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector2Int.right);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector2Int.down);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                HardDrop();
            }
            if (Time.time > stepTime)
            {
                Step();
            }
            this.board.Set(this);
        }

        private void Step()
        {
            stepTime = Time.time + stepDelay;

            // Step down to the next row
            Move(Vector2Int.down);

            // Once the piece has been inactive for too long it becomes locked
            if (lockTime >= lockDelay)
            {
                Lock();
            }
        }

        private void HardDrop()
        {
            while (Move(Vector2Int.down))
            {
                continue;
            }

            Lock();
        }

        private void Lock()
        {
            board.Set(this);
            board.ClearLines();
            board.SpawnPiece();
        }

        private bool Move(Vector2Int translation)
        {
            Vector3Int newPosition = this.position;
            newPosition.x += translation.x;
            newPosition.y += translation.y;

            bool valid = this.board.IsValidPosition(this, newPosition);

            if (valid)
            {
                this.position = newPosition;
                lockTime = 0f;
            }
            return valid;
        }

        private void Rotate(int direction)
        {
            // Store the current rotation in case the rotation fails
            // and we need to revert
            int originalRotation = rotationIndex;

            // Rotate all of the cells using a rotation matrix
            rotationIndex = Wrap(rotationIndex + direction, 0, 4);
            ApplyRotationMatrix(direction);

            // Revert the rotation if the wall kick tests fail
            if (!TestWallKicks(rotationIndex, direction))
            {
                rotationIndex = originalRotation;
                ApplyRotationMatrix(-direction);
            }
        }

        private void ApplyRotationMatrix(int direction)
        {
            for (int i = 0; i < this.cells.Length; i++)
            {
                Vector3 cell = this.cells[i];
                int x, y;
                switch (this.data.tetromino)
                {
                    case Tetromino.I:
                    case Tetromino.O:
                        cell.x -= 0.5f;
                        cell.y -= 0.5f;
                        x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                        y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                        break;

                    default:
                        x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                        y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                        break;
                }
                cells[i] = new Vector3Int(x, y, 0);
            }
        }

        private bool TestWallKicks(int rotationIndex, int rotationDirection)
        {
            int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

            for (int i = 0; i < data.wallKicks.GetLength(1); i++)
            {
                Vector2Int translation = data.wallKicks[wallKickIndex, i];

                if (Move(translation))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetWallKickIndex(int rotationIndex, int rotationDirection)
        {
            int wallKickIndex = rotationIndex * 2;

            if (rotationDirection < 0)
            {
                wallKickIndex--;
            }

            return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
        }

        //wrap: sarmak
        private int Wrap(int input, int min, int max)
        {
            if (input < min)
            {
                return max - (min - input) % (max - min);
            }
            else
            {
                return min + (input - min) % (max - min);
            }
        }
    }
}