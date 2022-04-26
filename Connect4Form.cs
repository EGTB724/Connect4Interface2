using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Connect4Interface
{
    struct Move {
        public string turn;
        public int row;
        public int col;
        public Move(string turn, int row, int col) {
            if(turn == "R") { 
                this.turn = "R";
            } else { 
                this.turn = "Y";
            }
            this.row = row;
            this.col = col;
        }
    }
    public partial class Connect4Form : Form
    {
        //Global variables
        //Used to represent the board numerically
        int[,] board;

        //Used to represent the board visually (with pictures)
        PictureBox[,] formBoard;

        //Used to store the log of the moves that have been made
        List<Move> moveLog = new List<Move>();
        int backLogIndex;

        //Indicates whose turn is is
        string turn;

        //Number of rows and columns in a traditional connect 4 board
        const int NUM_ROWS = 6;
        const int NUM_COLS = 7;
        const int FIVE_SECONDS_MILLI = 5000;
        const int TEN_SECONDS_MILLI = 10000;
        const int TWENTY_SECONDS_MILLI = 20000;
        const int ONE_MINUTE_MILLI = 60000;

        //Keeps track of the games won by both colors
        int redGamesWon = 0;
        int yellowGamesWon = 0;

        //State variable
        int state = 0;


        public Connect4Form()
        {
            InitializeComponent();
        }

        private void Connect4Form_Load(object sender, EventArgs e)
        {
            //Initialize global variables on load
            board = new int[,] { {0, 0, 0, 0, 0, 0, 0},
                                    {0, 0, 0, 0, 0, 0, 0},
                                    {0, 0, 0, 0, 0, 0, 0},
                                    {0, 0, 0, 0, 0, 0, 0},
                                    {0, 0, 0, 0, 0, 0, 0},
                                    {0, 0, 0, 0, 0, 0, 0}  };


            formBoard = new PictureBox[,] { { p00, p01, p02, p03, p04, p05, p06 },
                                            { p10, p11, p12, p13, p14, p15, p16 },
                                            { p20, p21, p22, p23, p24, p25, p26 },
                                            { p30, p31, p32, p33, p34, p35, p36 },
                                            { p40, p41, p42, p43, p44, p45, p46 },
                                            { p50, p51, p52, p53, p54, p55, p56 } };

            moveLog.Clear();
            turn = "R";
                        
            //Start with the game board disabled
            disableAllTiles();
            disableLogButtons();
        }

        private void StartOverButton_Click(object sender, EventArgs e)
        {
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLS; col++)
                {
                    //Any time you add an event handler, it's smart to subtract it first
                    //This will prevent multiple of the same event handlers piling up on the same object
                    formBoard[row, col].Click -= new EventHandler(clickTile);
                    formBoard[row, col].MouseEnter -= new EventHandler(enterTile);
                    formBoard[row, col].MouseLeave -= new EventHandler(leaveTile);
                    formBoard[row, col].Click += new EventHandler(clickTile);
                    formBoard[row, col].MouseEnter += new EventHandler(enterTile);
                    formBoard[row, col].MouseLeave += new EventHandler(leaveTile);
                    formBoard[row, col].BackgroundImage = null;
                    formBoard[row, col].Tag = null;
                    board[row, col] = 0;
                }
            }
            state++;
            disableLogButtons();
            moveLog.Clear();
            StartOverButton.Enabled = false;
            startGameToolStripMenuItem.Enabled = false;
            turnIndicator.BackgroundImage = Properties.Resources.redchip;
            turn = "R";
            redTurn();
        }

        private void ResetGameButton_Click(object sender, EventArgs e) {
            resetGame();
        }

        public void resetGame() {
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLS; col++)
                {
                    formBoard[row, col].Click -= new EventHandler(clickTile);
                    formBoard[row, col].MouseEnter -= new EventHandler(enterTile);
                    formBoard[row, col].MouseLeave -= new EventHandler(leaveTile);
                    formBoard[row, col].BackgroundImage = null;
                    formBoard[row, col].Tag = null;
                    board[row, col] = 0;
                }
            }
            state++;
            disableLogButtons();
            moveLog.Clear();
            turnIndicator.BackgroundImage = null;
            StartOverButton.Enabled = true;
            startGameToolStripMenuItem.Enabled = true;
        }

        private void leaveTile(object sender, EventArgs e)
        {
            PictureBox tile = sender as PictureBox;

            if (tile == null)
            {
                return;
            }

            //Check if background image is the highlight
            if ((string)tile.Tag == "highlight") {
                tile.BackgroundImage = null;
                tile.Tag = "";
            }
            
            return;
        }

        private void enterTile(object sender, EventArgs e)
        {
            PictureBox tile = sender as PictureBox;

            if (tile == null)
            {
                return;
            }

            if (tile.BackgroundImage == null)
            {
                if (isValid(tile.Name))
                {
                    tile.BackgroundImage = Properties.Resources.highlight;
                    tile.Tag = "highlight";
                }
            }

            return;
        }

        private void clickTile(object sender, EventArgs e)
        {
            PictureBox tile = sender as PictureBox;

            if (tile == null)
            {
                return;
            }

            if (turn == "R")
            {
                if (isValid(tile.Name)) 
                {
                    //Place the piece on both boards
                    placePiece(tile.Name);
                    tile.BackgroundImage = Properties.Resources.redchip;
                    tile.Tag = "R";

                    //Check if the player has won
                    if (gameOver())
                    {
                        return;
                    }
                    else
                    {

                        //Change the turn
                        turnIndicator.BackgroundImage = Properties.Resources.yellowchip;
                        turn = "Y";
                        flipBoard();
                        yellowTurn();
                        return;
                    }
                }
                
            }
            else if (turn == "Y")
            {
                if (isValid(tile.Name))
                {
                    //Place the piece on both boards
                    placePiece(tile.Name);
                    tile.BackgroundImage = Properties.Resources.yellowchip;
                    tile.Tag = "Y";

                    //Check if the player has won
                    if (gameOver())
                    {
                        return;
                    }
                    else
                    {

                        //Change the turn
                        turnIndicator.BackgroundImage = Properties.Resources.redchip;
                        turn = "R";
                        flipBoard();
                        redTurn();
                        return;
                    }
                }
            }
        }

        private async void yellowTurn() 
        {
            if (YellowPlayerLabel.Text == "Human")
            {
                enableAllTiles();
            }
            else {
                //If we are here, a computer is playing
                disableAllTiles();

                //We need to output the current board state 
                outputBoard();

                
                int currState = state;

                //Call the executable
                bool processExited = await Task.Run(() => runExecutable());
                if (state != currState) 
                {
                    Console.WriteLine("State changed");
                    return;
                }
                if(processExited == false) {
                    quitGameTimeLimit();
                    return;
                }

                //Take in the move played by the computer
                //Get move.txt from the working directory of the computer player
                string directory = Path.GetDirectoryName(YellowPlayerLabel.Text);
                string text = System.IO.File.ReadAllText(directory + "/move.txt");

                int row = (int)(text[0] - '0');
                int col = (int)(text[2] - '0');

                //Make sure the move is legal
                if (!isLegal(row, col))
                {
                    ++redGamesWon;
                    updateScoreLabel();

                    string message = "Red won because Yellow made an illegal move";
                    MessageBox.Show(message);

                    gameStopped();
                    return;
                }

                board[row, col] = 1;
                formBoard[row,col].BackgroundImage = Properties.Resources.yellowchip;
                formBoard[row,col].Tag = "Y";

                Move nextMove = new Move(turn, row, col);
                moveLog.Add(nextMove);

                //Check if the player has won
                if (gameOver())
                {
                    return;
                }

                //Change the turn
                turn = "R";
                flipBoard();
                redTurn();
                return;
            }
        }

        private async void redTurn() 
        {
            if (RedPlayerLabel.Text == "Human")
            {
                enableAllTiles();
            }
            else
            {
                //If we are here, a computer is playing
                disableAllTiles();

                //We need to output the current board state 
                outputBoard();

                int currState = state;

                //Call the executable
                bool processExited = await Task.Run(() => runExecutable());
                if (state != currState)
                {
                    Console.WriteLine("State changed");
                    return;
                }
                if (processExited == false) {
                    quitGameTimeLimit();
                    return;
                }

                //Take in the move played by the computer
                //Get move.txt from the working directory of the computer player
                string directory = Path.GetDirectoryName(RedPlayerLabel.Text);
                string text = System.IO.File.ReadAllText(directory + "/move.txt");

                int row = (int)(text[0] - '0');
                int col = (int)(text[2] - '0');

                //Make sure the move is legal
                if (!isLegal(row, col)) {
                    ++yellowGamesWon;
                    updateScoreLabel();

                    string message = "Yellow won because Red made an illegal move";
                    MessageBox.Show(message);

                    gameStopped();
                    return;
                }

                board[row, col] = 1;
                formBoard[row, col].BackgroundImage = Properties.Resources.redchip;
                formBoard[row, col].Tag = "R";
                
                Move nextMove = new Move(turn, row, col);
                moveLog.Add(nextMove);

                //Check if the player has won
                if (gameOver())
                {
                    return;
                }

                //Change the turn
                turn = "Y";
                flipBoard();
                yellowTurn();
                return;
            }
        }

        public bool runExecutable() {
            Process process = new System.Diagnostics.Process();
            if(turn == "R") { 
                process.StartInfo.FileName = RedPlayerLabel.Text;

                //You have to manually set the working directory of the called executable or else it will assume its the same as the c# program
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(RedPlayerLabel.Text);

                //Check if the show console window checkbox is check or not
                if (!RedConsoleWindowCheckBox.Checked) {
                    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
            } else { 
                process.StartInfo.FileName = YellowPlayerLabel.Text;

                //You have to manually set the working directory of the called executable or else it will assume its the same as the c# program
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(YellowPlayerLabel.Text);

                //Check if the show console window checkbox is check or not
                if (!YellowConsoleWindowCheckBox.Checked)
                {
                    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
            }            
            return startAndExitProcess(process);
        }

        public bool startAndExitProcess(Process process) {
            bool processExited;
            if(FiveSeconds_LimitMenuItem.Checked) { 
                process.Start();
                processExited = process.WaitForExit(FIVE_SECONDS_MILLI);
            } else if(TenSeconds_LimitMenuItem.Checked) { 
                process.Start();
                processExited = process.WaitForExit(TEN_SECONDS_MILLI);
            } else if(TwentySeconds_LimitMenuItem.Checked) { 
                process.Start();
                processExited = process.WaitForExit(TWENTY_SECONDS_MILLI);
            } else if(OneMinute_LimitMenuItem.Checked) { 
                process.Start();
                processExited = process.WaitForExit(ONE_MINUTE_MILLI);
            } else { 
                process.Start();
                process.WaitForExit();
                processExited = true;
            }
            return processExited;
        }

        public void quitGameTimeLimit() {
            string message = "";
            if(turn == "R") { 
                message += "Game exited because Red took too long to run.";
            } else { 
                message += "Game exited because Yellow took too long to run.";
            }
            MessageBox.Show(message);
            gameStopped();
        }

        //Takes in the name of the tile and returns whether it would be valid to play there
        private bool isValid(string tileName)
        {
            int row = (int)(tileName[1] - '0');
            int col = (int)(tileName[2] - '0');

            //Make sure the space isn't already occupied
            if (board[row, col] != 0) 
            {
                return false;
            }

            //Moves on the bottom are valid
            if (row == 5) 
            {
                return true;
            }

            //If not on bottom then check if there's a piece below
            if (board[row + 1, col] != 0) {
                return true;
            }

            return false;
        }

        //Check if the move is legal
        private bool isLegal(int row, int col)
        {
            //Check if the spot is open
            if (board[row, col] != 0) {
                return false;
            }

            //Check if we're in the bottom row
            if (row == NUM_ROWS - 1) {
                return true;
            }

            //If we're not in the borrom row, make sure there's a piece below us
            if (board[row + 1, col] != 0) {
                return true;
            }
            else {
                return false;
            }
        }

        //Takes in the name of the tile and adds the value to array 
        private void placePiece(string tileName) 
        {
            int row = (int)(tileName[1] - '0');
            int col = (int)(tileName[2] - '0');

            board[row, col] = 1;
            Move nextMove = new Move(turn, row, col);
            moveLog.Add(nextMove);
        }

        private bool gameOver() {
            if(boardFull()) {
                tieGame();
                gameStopped();
                return true;
            } else if(hasWon()){
                playerWon();
                gameStopped();
                return true;
            }
            return false;
        }

        private bool boardFull() {
            bool boardFull = true;
            for(int i=0; i<NUM_ROWS; i++) { 
                for(int j=0; j<NUM_COLS; j++) { 
                    if(board[i,j] == 0) {
                        boardFull = false;
                    }
                }
            }
            return boardFull;
        }

        private void tieGame() {
            MessageBox.Show("TIE GAME");
        }

        //Check the board to see if there is any line of 4 1's
        private bool hasWon()
        {
            // horizontalCheck 
            for (int j = 0; j < NUM_COLS - 3; j++)
            {
                for (int i = 0; i < NUM_ROWS; i++)
                {
                    if (board[i, j] == 1 && board[i,j + 1] == 1 && board[i,j + 2] == 1 && board[i,j + 3] == 1)
                    {
                        return true;
                    }
                }
            }
            // verticalCheck
            for (int i = 0; i < NUM_ROWS - 3; i++)
            {
                for (int j = 0; j < NUM_COLS; j++)
                {
                    if (board[i,j] == 1 && board[i + 1,j] == 1 && board[i + 2,j] == 1 && board[i + 3,j] == 1)
                    {
                        return true;
                    }
                }
            }
            // ascendingDiagonalCheck 
            for (int i = 3; i < NUM_ROWS; i++)
            {
                for (int j = 0; j < NUM_COLS - 3; j++)
                {
                    if (board[i,j] == 1 && board[i - 1,j + 1] == 1 && board[i - 2,j + 2] == 1 && board[i - 3,j + 3] == 1)
                        return true;
                }
            }
            // descendingDiagonalCheck
            for (int i = 3; i < NUM_ROWS; i++)
            {
                for (int j = 3; j < NUM_COLS; j++)
                {
                    if (board[i,j] == 1 && board[i - 1,j - 1] == 1 && board[i - 2,j - 2] == 1 && board[i - 3,j - 3] == 1)
                        return true;
                }
            }
            return false;
        }


        public void playerWon() {
            string winnerColor;
            if(turn == "R") {
                winnerColor = "Red";
                redGamesWon++;
            } else {
                winnerColor = "Yellow";
                yellowGamesWon++;
            }
            
            updateScoreLabel();

            MessageBox.Show(winnerColor + " player won!!");

            //string message = "";
            //for(int i=0; i<moveLog.Count; i++) { 
            //    message += "Move " + i + ": [" + moveLog[i].row + "][" + moveLog[i].col + "]";
            //}
            //MessageBox.Show(message);
        }

        public void gameStopped() {
            disableAllTiles();
            BackLogButton.Enabled = true;
            backLogIndex = moveLog.Count;
        }

        public void updateScoreLabel() {
            YellowScoreLabel.Text = "Games Won: " + yellowGamesWon.ToString();
            RedScoreLabel.Text = "Games Won: " + redGamesWon.ToString();
        }

        //Flips the array (swaps 1 and -1)
        private void flipBoard() 
        {
            for (int row = 0; row < NUM_ROWS; row++) {
                for (int col = 0; col < NUM_COLS; col++) {
                    if (board[row, col] == 1)
                    {
                        board[row, col] = -1;
                    }
                    else if (board[row, col] == -1) 
                    {
                        board[row, col] = 1;
                    }
                }
            }
        }

        private void disableAllTiles() 
        {
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLS; col++)
                {
                    formBoard[row, col].Click -= new EventHandler(clickTile);
                    formBoard[row, col].MouseEnter -= new EventHandler(enterTile);
                    formBoard[row, col].MouseLeave -= new EventHandler(leaveTile);
                }
            }
        }

        private void enableAllTiles() 
        {
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLS; col++)
                {
                    formBoard[row, col].Click -= new EventHandler(clickTile);
                    formBoard[row, col].MouseEnter -= new EventHandler(enterTile);
                    formBoard[row, col].MouseLeave -= new EventHandler(leaveTile);
                    formBoard[row, col].Click += new EventHandler(clickTile);
                    formBoard[row, col].MouseEnter += new EventHandler(enterTile);
                    formBoard[row, col].MouseLeave += new EventHandler(leaveTile);
                }
            }
        }

        private void YellowComputerButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog()
            {
                Filter = "Executables|*.exe"
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                //Returns the path + filename 
                string filename = openFileDialog1.FileName;
                //MessageBox.Show(filename);
                //Process.Start(filename);
                YellowPlayerLabel.Text = filename;
            }
        }

        private void YellowHumanButton_Click(object sender, EventArgs e)
        {
            YellowPlayerLabel.Text = "Human";
        }

        private void RedComputerButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog()
            {
                Filter = "Executables|*.exe"
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Returns the path + filename 
                string filename = openFileDialog1.FileName;
                RedPlayerLabel.Text = filename;
            }
        }

        private void RedHumanButton_Click(object sender, EventArgs e)
        {
            RedPlayerLabel.Text = "Human";
        }

        private void outputBoard() 
        {
            string directory = "";
            string outputFile = "board.txt";
            string boardString = "";

            //We want to put the board.txt in the location of the computer .exe, NOT in our own directory
            //This is because we should not assume that the computer .exe is in the same directory as us
            if (turn == "R")
            {
                directory = Path.GetDirectoryName(RedPlayerLabel.Text);
            }
            else
            {
                directory = Path.GetDirectoryName(YellowPlayerLabel.Text);
            }
            outputFile = directory + "/" + outputFile;

            for (int row = 0; row < NUM_ROWS; row++) 
            {
                for (int col = 0; col < NUM_COLS; col++) 
                {
                    //O is max player
                    //X is min player
                    //. is an open spot
                    if (board[row, col] == 1)
                    {
                        boardString += "O";
                    }
                    else if (board[row, col] == -1)
                    {
                        boardString += "X";
                    }
                    else if (board[row, col] == 0) 
                    {
                        boardString += ".";
                    }
                }
            }

            try
            {
                // Check if file already exists. If yes, delete it.     
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                //Create the new file
                using (StreamWriter sw = File.CreateText(outputFile))
                {
                    sw.WriteLine(boardString);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
        }

        private void ForwardLogButton_Click(object sender,EventArgs e) {
            stepForwardInLog();
        }

        private void BackLogButton_Click(object sender,EventArgs e) {
            stepBackInLog();
        }

        public void disableLogButtons() {
            ForwardLogButton.Enabled = false;
            BackLogButton.Enabled = false;
            forwardInLogToolStripMenuItem.Enabled = false;
            backwardInLogToolStripMenuItem.Enabled = false;
        }

        public void stepForwardInLog() {
            int row = moveLog[backLogIndex].row;
            int col = moveLog[backLogIndex].col;
            string turn = moveLog[backLogIndex].turn;

            if(turn == "R") {
                formBoard[row, col].BackgroundImage = Properties.Resources.redchip;
            } else if(turn == "Y") { 
                formBoard[row, col].BackgroundImage = Properties.Resources.yellowchip;
            }
            BackLogButton.Enabled = true;
            backwardInLogToolStripMenuItem.Enabled = true;
            
            backLogIndex++;
            if(backLogIndex >= moveLog.Count) {
                ForwardLogButton.Enabled = false;
                forwardInLogToolStripMenuItem.Enabled = false;
            }
        }

        public void stepBackInLog() {
            backLogIndex--;
            int row = moveLog[backLogIndex].row;
            int col = moveLog[backLogIndex].col;

            formBoard[row, col].BackgroundImage = null;
            ForwardLogButton.Enabled = true;
            forwardInLogToolStripMenuItem.Enabled = true;

            if(backLogIndex <= 0) { 
                BackLogButton.Enabled = false;
                backwardInLogToolStripMenuItem.Enabled = false;
            }
        }

        private void FiveSeconds_LimitMenuItem_Click(object sender,EventArgs e) {
           if(FiveSeconds_LimitMenuItem.Checked == false) { 
                FiveSeconds_LimitMenuItem.Checked = true;

                TenSeconds_LimitMenuItem.Checked = false;
                TwentySeconds_LimitMenuItem.Checked = false;
                OneMinute_LimitMenuItem.Checked = false;
                NoLimit_LimitMenuItem.Checked = false;
            }
        }

        private void TenSeconds_LimitMenuItem_Click(object sender,EventArgs e) {
            if(TenSeconds_LimitMenuItem.Checked == false) { 
                TenSeconds_LimitMenuItem.Checked = true;
                
                FiveSeconds_LimitMenuItem.Checked = false;
                TwentySeconds_LimitMenuItem.Checked = false;
                OneMinute_LimitMenuItem.Checked = false;
                NoLimit_LimitMenuItem.Checked = false;
            }
        }

        private void TwentySeconds_LimitMenuItem_Click(object sender,EventArgs e) {
            if(TwentySeconds_LimitMenuItem.Checked == false) { 
                TwentySeconds_LimitMenuItem.Checked = true;
                
                FiveSeconds_LimitMenuItem.Checked = false;
                TenSeconds_LimitMenuItem.Checked = false;
                OneMinute_LimitMenuItem.Checked = false;
                NoLimit_LimitMenuItem.Checked = false;
            }
        }

        private void OneMinut_LimitMenuItem_Click(object sender,EventArgs e) {
            if(OneMinute_LimitMenuItem.Checked == false) { 
                OneMinute_LimitMenuItem.Checked = true;
                
                FiveSeconds_LimitMenuItem.Checked = false;
                TenSeconds_LimitMenuItem.Checked = false;
                TwentySeconds_LimitMenuItem.Checked = false;
                NoLimit_LimitMenuItem.Checked = false;
            }
        }

        private void NoLimit_LimitMenuItem_Click(object sender, EventArgs e)
        {
            if (NoLimit_LimitMenuItem.Checked == false)
            {
                NoLimit_LimitMenuItem.Checked = true;

                FiveSeconds_LimitMenuItem.Checked = false;
                TenSeconds_LimitMenuItem.Checked = false;
                TwentySeconds_LimitMenuItem.Checked = false;
                OneMinute_LimitMenuItem.Checked = false;
            }
        }

            private void SaveLog_FileMenuItem_Click(object sender,EventArgs e) {
            string logString = "";
            for(int i=0; i<moveLog.Count; i++) { 
                logString += moveLog[i].turn + " " + moveLog[i].row + " " + moveLog[i].col + "\n";              
            }
            byte[] arr = Encoding.ASCII.GetBytes(logString);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            Stream saveFileStream;

            saveFileDialog.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;

            if(saveFileDialog.ShowDialog() == DialogResult.OK) {
                if((saveFileStream = saveFileDialog.OpenFile()) != null) {
                    saveFileStream.Write(arr, 0, arr.Length);
                    saveFileStream.Close();
                }
            }
        }

        private void OpenLog_FileMenuItem_Click(object sender,EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt";    
            Stream openFileStream;
            List<string> logLines = new List<string>();

            if (openFileDialog.ShowDialog() == DialogResult.OK) { 
                openFileStream = openFileDialog.OpenFile();
                StreamReader reader = new StreamReader(openFileStream);
                while(reader.Peek() != -1) { 
                    logLines.Add(reader.ReadLine());
                }
                resetGame();
                convertIntoMoveLog(logLines);
                backLogIndex = 0;
                ForwardLogButton.Enabled = true;
                forwardInLogToolStripMenuItem.Enabled = true;
            }
        }

        private void convertIntoMoveLog(List<string> logLines) {
            for(int i=0; i<logLines.Count; i++) { 
                string line = logLines[i];
                string turn = line.Substring(0,1);
                string rowString = line.Substring(2,1);
                string colString = line.Substring(4,1);
                int row = int.Parse(rowString);
                int col = int.Parse(colString);                
                Move move = new Move(turn, row, col);
                moveLog.Add(move);
            }
            
            string message = "";
            for(int k=0; k<moveLog.Count; k++)
            {
                message += moveLog[k].turn + " " + moveLog[k].row + " " + moveLog[k].col + "\n";
            }
            MessageBox.Show(message);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
               "Connect4 Interface\n" +
               "Created by: Ethan Boulanger & Nathan Cauley\n" +
               "For use at Southern Illinois University Edwardsville - Artificial Intelligence Class\n" +
               "Copyright © 2022 Ethan Boulanger and Nathan Cauley",
               "About Connect4 Interface"
               );
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            Application.Exit();
        }

        private void Connect4Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
