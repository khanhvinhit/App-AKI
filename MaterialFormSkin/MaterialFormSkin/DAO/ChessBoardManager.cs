using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialFormSkin.DTO;
using MaterialSkin.Controls;

namespace MaterialFormSkin.DAO
{
    public class ChessBoardManager
    {
        #region properties

        private Panel chessBoard;

        public Panel ChessBoard
        {
            get { return chessBoard; }
            set { chessBoard = value; }
        }
        private List<Player> player;
        public List<Player> Player
        {
            get { return player; }
            set { player = value; }
        }
        private int currentPlayer;
        public int CurrentPlayer
        {
            get { return currentPlayer; }
            set { currentPlayer = value; }
        }
        private MaterialSingleLineTextField playerName;
        public MaterialSingleLineTextField PlayerName
        {
            get { return playerName; }
            set { playerName = value; }
        }


        private PictureBox mark;
        public PictureBox Mark
        {
            get { return mark; }
            set { mark = value; }
        }
        private List<List<Button>> matrix;
        public List<List<Button>> Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }



        private event EventHandler playerMarked;
        public event EventHandler PlayerMarked
        {
            add { playerMarked += value; }
            remove { playerMarked -= value; }

        }

        private event EventHandler endedGame;
        public event EventHandler EndedGame
        {
            add { endedGame += value; }
            remove { endedGame -= value; }

        }

        private Stack<PlayInfo> playTimeLine;
        public Stack<PlayInfo> PlayTimeLine
        {
            get { return playTimeLine; }
            set { playTimeLine = value; }
        }
        #endregion

        #region initialize

        public ChessBoardManager(Panel chess, MaterialSingleLineTextField playerName, PictureBox mark)
        {
            this.ChessBoard = chess;
            this.PlayerName = playerName;
            this.Mark = mark;
            this.Player = new List<Player>()
            {
                new Player("Vinh",Properties.Resources.o),
                new Player("Khánh",Properties.Resources.x)
            };
            
        }
        #endregion

        #region methhod
        public void DrawChessBoard()
        {
            chessBoard.Enabled = true;
            chessBoard.Controls.Clear();

            playTimeLine = new Stack<PlayInfo>();

            CurrentPlayer = 0;
            ChangePlayer();
            Matrix = new List<List<Button>>();
            Button old = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int j = 0; j < DAO.Cons.CHESS_BOARD_WIDTH; j++)
            {
                Matrix.Add(new List<Button>());
                for (int k = 0; k < DAO.Cons.CHESS_BOARD_WIDTH; k++)
                {
                    Button btn = new Button();
                    btn.Width = DAO.Cons.CHESS_WIDTH;
                    btn.Height = DAO.Cons.CHESS_HEIGHT;
                    btn.Location = new Point(old.Location.X + old.Width, old.Location.Y);
                    btn.Tag = j.ToString();

                    btn.Click += btn_Click;
                    ChessBoard.Controls.Add(btn);
                    Matrix[j].Add(btn);
                    old = btn;
                }
                old.Location = new Point(0, old.Location.Y + DAO.Cons.CHESS_HEIGHT);
                old.Width = 0;
                old.Height = 0;
            }



        }
        #endregion
        #region Event

        void btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.BackgroundImage != null) return;
            MarkB(btn);

            PlayTimeLine.Push(new PlayInfo(GetChessPoint(btn), currentPlayer));
            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;
            ChangePlayer();

            if (playerMarked != null)
            {
                playerMarked(this, new EventArgs());
            }

            if (isEndGame(btn))
            {
                EndGame();
            }
        }

        public void EndGame()
        {
            if (endedGame != null)
            {
                endedGame(this, new EventArgs());
            }
            //MessageBox.Show("Kết thúc");
        }


        public bool Undo()
        {
            if (PlayTimeLine.Count <= 0)
            {
                return false;
            }
            PlayInfo oldpoint = playTimeLine.Pop();
            Button btn = Matrix[oldpoint.Point.Y][oldpoint.Point.X];

            btn.BackgroundImage = null;

            
            if (PlayTimeLine.Count <= 0)
            {
                
                CurrentPlayer = 0;
            }
            else
            {
                oldpoint = playTimeLine.Peek();
                CurrentPlayer = oldpoint.CurrentPlayer == 1 ? 0 : 1;
            }

            ChangePlayer();
            return true;
        }
        private bool isEndGame(Button btn)
        {
            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimary(btn) || isEndSub(btn);
        }
        private Point GetChessPoint(Button btn)
        {


            int vertical = Convert.ToInt32((btn.Tag));
            int horizontal = Matrix[vertical].IndexOf(btn);
            Point point = new Point(horizontal, vertical);
            return point;
        }
        private bool isEndHorizontal(Button btn)
        {
            Point point = GetChessPoint(btn);
            int countLeft = 0;
            for (int i = point.X; i >= 0; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countLeft++;
                }
                else
                {
                    break;
                }
            }
            int countRight = 0;
            for (int i = point.X + 1; i < DAO.Cons.CHESS_BOARD_WIDTH; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countRight++;
                }
                else
                {
                    break;
                }
            }
            return countLeft + countRight == 5;
        }
        private bool isEndVertical(Button btn)
        {
            Point point = GetChessPoint(btn);
            int countTop = 0;
            for (int i = point.X; i >= 0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                {
                    break;
                }
            }
            int countBottom = 0;
            for (int i = point.X + 1; i < DAO.Cons.CHESS_BOARD_WIDTH; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                {
                    break;
                }
            }
            return countTop + countBottom == 5;
        }
        private bool isEndPrimary(Button btn)
        {
            Point point = GetChessPoint(btn);
            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0)
                {
                    break;
                }
                if (Matrix[point.Y - i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                {
                    break;
                }
            }
            int countBottom = 0;
            for (int i = 1; i <= Cons.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.X + i >= Cons.CHESS_BOARD_WIDTH || point.Y + i >= Cons.CHESS_BOARD_WIDTH)
                {
                    break;
                }
                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                {
                    break;
                }
            }
            return countTop + countBottom == 5;
        }
        private bool isEndSub(Button btn)
        {
            Point point = GetChessPoint(btn);
            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X + i > Cons.CHESS_BOARD_WIDTH || point.Y - i < 0)
                {
                    break;
                }
                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                {
                    break;
                }
            }
            int countBottom = 0;
            for (int i = 1; i <= Cons.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.X - i < 0 || point.Y + i >= Cons.CHESS_BOARD_WIDTH)
                {
                    break;
                }
                if (Matrix[point.Y + i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                {
                    break;
                }
            }
            return countTop + countBottom == 5;
        }

        private void MarkB(Button btn)
        {
            btn.BackgroundImage = Player[CurrentPlayer].Mark;

            btn.BackgroundImageLayout = ImageLayout.Zoom;
        }

        private void ChangePlayer()
        {
            PlayerName.Text = Player[CurrentPlayer].Name;
            Mark.Image = Player[CurrentPlayer].Mark;
        }
        #endregion
    }
}
