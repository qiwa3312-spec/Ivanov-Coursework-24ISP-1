using System;
using System.Drawing;
using System.Windows.Forms;

namespace Найди_пару
{
    public partial class Form1 : Form
    {
        // Таймеры для игры
        private Timer gameTimer;
        private Timer flipTimer;
        
        // Переменные состояния игры
        private int timeLeft;
        private int attemptsLeft;
        private int pairsFound;
        private int totalPairs;
        
        // Ссылки на нажатые карточки
        private Button firstClicked;
        private Button secondClicked;
        
        private Random random = new Random();

        public Form1()
        {
            InitializeComponent();
            
            // Настройка главного таймера (1 секунда)
            gameTimer = new Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;

            // Настройка таймера переворота (0.75 секунды)
            flipTimer = new Timer();
            flipTimer.Interval = 750;
            flipTimer.Tick += FlipTimer_Tick;

            button2.Click += ButtonStart_Click;
            
            button1.Visible = false; 

            radioButton1.Checked = true;
            
            // Обновление интерфейса
            bool isCustom = !radioButton1.Checked;
            label4.Visible = isCustom;
            label5.Visible = isCustom;
            label6.Visible = isCustom;
            textBox1.Visible = isCustom;
            textBox2.Visible = isCustom;
            textBox3.Visible = isCustom;

            // Защита полей
            textBox1.KeyPress += textBox_KeyPress;
            textBox2.KeyPress += textBox_KeyPress;
            textBox3.KeyPress += textBox_KeyPress;
        }

        // Защита полей
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            // Обновление интерфейса
            bool isCustom = !radioButton1.Checked;
            label4.Visible = isCustom;
            label5.Visible = isCustom;
            label6.Visible = isCustom;
            textBox1.Visible = isCustom;
            textBox2.Visible = isCustom;
            textBox3.Visible = isCustom;
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            // Значения по умолчанию
            int rows = 4, cols = 4;
            timeLeft = 60;
            attemptsLeft = 20;

            // Если выбрана стандартная сложность
            if (radioButton1.Checked) 
            {
                string difficulty = domainUpDown1.Text;
                if (difficulty == "лёгкая") { rows = 4; cols = 4; attemptsLeft = 20; timeLeft = 60; }
                else if (difficulty == "средняя") { rows = 6; cols = 6; attemptsLeft = 40; timeLeft = 120; }
                else if (difficulty == "сложная") { rows = 6; cols = 8; attemptsLeft = 60; timeLeft = 180; }
            }
            else // Если своя сложность
            {
                string dim = textBox1.Text.Trim();
                
                if (dim == "") dim = "4";
                int size = Convert.ToInt32(dim);
                
                // Проверка диапазона размера поля
                if (size < 2 || size > 10)
                {
                    MessageBox.Show("Размер поля должен быть от 2 до 10.", "Ошибка");
                    return;
                }
                rows = size; 
                cols = size;

                // Проверка на четность количества карточек
                if ((rows * cols) % 2 != 0)
                {
                    MessageBox.Show("Общее количество карточек должно быть четным!", "Ошибка");
                    return;
                }

                // Считываем попытки
                if (textBox2.Text != "") attemptsLeft = Convert.ToInt32(textBox2.Text);
                else attemptsLeft = 20;

                // Считываем время
                if (textBox3.Text != "") timeLeft = Convert.ToInt32(textBox3.Text);
                else timeLeft = 60;
            }
            // Начало игры
            totalPairs = (rows * cols) / 2;
            pairsFound = 0;
            firstClicked = null;
            secondClicked = null;

            // Обновление статистики
            label2.Text = string.Format("Осталось попыток: {0}", attemptsLeft);
            label3.Text = string.Format("Найдено: {0} из {1}", pairsFound, totalPairs);
            label1.Text = string.Format("Time: {0}", timeLeft);

            // Очистка
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            // Настройка сетки
            tableLayoutPanel1.RowCount = rows;
            tableLayoutPanel1.ColumnCount = cols;
            // Настройка и добавление столбцов/Строк
            for (int i = 0; i < rows; i++)
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
            for (int i = 0; i < cols; i++)
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));

            //Пути к карточкам
            string[] possibleIcons = new string[] 
            { 
                "../../Images/icon_1.png", "../../Images/icon_2.png", "../../Images/icon_3.png", "../../Images/icon_4.png", "../../Images/icon_5.png",
                "../../Images/icon_6.png", "../../Images/icon_7.png", "../../Images/icon_8.png", "../../Images/icon_9.png", "../../Images/icon_10.png",
                "../../Images/icon_11.png", "../../Images/icon_12.png", "../../Images/icon_13.png", "../../Images/icon_14.png", "../../Images/icon_15.png",
                "../../Images/icon_16.png", "../../Images/icon_17.png", "../../Images/icon_18.png", "../../Images/icon_19.png", "../../Images/icon_20.png"
            };
            
            string[] icons = new string[totalPairs * 2];
            int currentIndex = 0;
            
            // Заполнение массива парами путей к картинкам
            for(int i = 0; i < totalPairs; i++)
            {
                string icon = possibleIcons[i % possibleIcons.Length];
                icons[currentIndex] = icon;
                icons[currentIndex + 1] = icon;
                currentIndex += 2;
            }

            // Алгоритм перемешивания массива
            for (int i = 0; i < icons.Length; i++)
            {
                int j = random.Next(i, icons.Length);
                string temp = icons[i];
                icons[i] = icons[j];
                icons[j] = temp;
            }
            
            // Создание карточек на поле
            for (int i = 0; i < rows * cols; i++)
            {
                Button btn = new Button();
                btn.Dock = DockStyle.Fill;
                btn.Margin = new Padding(2);
                
                // Режим отображения картинки 
                btn.BackgroundImageLayout = ImageLayout.Zoom;
                
                btn.Tag = icons[i];
                btn.Text = "";
                btn.BackColor = Color.LightGray;
                
                btn.Click += Card_Click;
                
                tableLayoutPanel1.Controls.Add(btn);
            }

            gameTimer.Start();
        }

        private void Card_Click(object sender, EventArgs e)
        {
            if (flipTimer.Enabled) return; 

            Button clickedButton = sender as Button;

            // Проверка картини
            if (clickedButton == null || clickedButton.BackgroundImage != null) return; 

            // Если не открыта первая карточка
            if (firstClicked == null)
            {
                firstClicked = clickedButton;
                try { firstClicked.BackgroundImage = Image.FromFile(firstClicked.Tag.ToString()); } catch { }
                firstClicked.BackColor = Color.White;
                return;
            }

            // Если открыта вторая карточка
            secondClicked = clickedButton;
            try { secondClicked.BackgroundImage = Image.FromFile(secondClicked.Tag.ToString()); } catch { }
            secondClicked.BackColor = Color.White;

            // === ПРОВЕРКА СОВПАДЕНИЯ ===
            if (firstClicked.Tag.ToString() == secondClicked.Tag.ToString())
            {
                // Карточки совпали
                pairsFound++;
                firstClicked = null;
                secondClicked = null;
                
                // Обновление статистики
                label2.Text = string.Format("Осталось попыток: {0}", attemptsLeft);
                label3.Text = string.Format("Найдено: {0} из {1}", pairsFound, totalPairs);
                label1.Text = string.Format("Time: {0}", timeLeft);

                // Если найдены все пары
                if (pairsFound == totalPairs)
                {
                    gameTimer.Stop();
                    MessageBox.Show(string.Format("Поздравляем! Вы выиграли!\nОсталось попыток: {0}\nОсталось времени: {1} сек", attemptsLeft, timeLeft), "Победа!");
                }
            }
            else
            {
                // Карточки не совпали
                attemptsLeft--;
                
                // Обновление статистики
                label2.Text = string.Format("Осталось попыток: {0}", attemptsLeft);
                label3.Text = string.Format("Найдено: {0} из {1}", pairsFound, totalPairs);
                label1.Text = string.Format("Time: {0}", timeLeft);

                // Если вышло время
                if (timeLeft <= 0)
                {
                    gameTimer.Stop();
                    MessageBox.Show("Время вышло! Вы проиграли.", "Поражение");

                    // Показываем все карточки
                    for (int i = 0; i < tableLayoutPanel1.Controls.Count; i++)
                    {
                        Button btn = tableLayoutPanel1.Controls[i] as Button;
                        if (btn != null)
                        {
                            try { btn.BackgroundImage = Image.FromFile(btn.Tag.ToString()); } catch { }
                            btn.BackColor = Color.White;
                        }
                    }
                }
                else flipTimer.Start(); // Ждем 0.75 сек и переворачиваем
            }
        }

        private void FlipTimer_Tick(object sender, EventArgs e)
        {
            flipTimer.Stop();
            
            // Возвращаем карточкам вид "рубашки" убирая картинку
            firstClicked.BackgroundImage = null;
            firstClicked.BackColor = Color.LightGray;
            secondClicked.BackgroundImage = null;
            secondClicked.BackColor = Color.LightGray;
            
            firstClicked = null;
            secondClicked = null;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            timeLeft--;
            
            // Обновление статистики
            label2.Text = string.Format("Осталось попыток: {0}", attemptsLeft);
            label3.Text = string.Format("Найдено: {0} из {1}", pairsFound, totalPairs);
            label1.Text = string.Format("Time: {0}", timeLeft);
            
            // Если вышло время
            if (timeLeft <= 0)
            {
                gameTimer.Stop();
                MessageBox.Show("Время вышло! Вы проиграли.", "Поражение");

                // Показываем все карточки
                for (int i = 0; i < tableLayoutPanel1.Controls.Count; i++)
                {
                    Button btn = tableLayoutPanel1.Controls[i] as Button;
                    if (btn != null)
                    {
                        try { btn.BackgroundImage = Image.FromFile(btn.Tag.ToString()); } catch { }
                        btn.BackColor = Color.White;
                    }
                }
            }
        }
    }
}
