using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;


namespace Scanner
{
    public partial class FormGUI : Form
    {
        // Указатель на окно редактирования
        private RichTextBox inputBox;
        // Лист всех ошибок из всех файлов
        private List<ReportFile> report = new List<ReportFile>();
        private List<ReportFile> reportANTLR = new List<ReportFile>();

        public FormGUI()
        {
            InitializeComponent();

            outputBoxTetrad.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            outputBoxParser.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            files.TabPages.Add(NewFile("Новый документ" + (files.TabCount + 1).ToString()));
        }

        // Меню - Файл

        private void CreatFile_Click(object sender, EventArgs e)
        {
            files.TabPages.Add(NewFile("Новый документ" + (files.TabCount + 1).ToString()));
        }
        private TabPage NewFile(string name)
        {
            TabPage file = new TabPage(name);
            file.BorderStyle = BorderStyle.FixedSingle;

            RichTextBox inputBoxFile = new RichTextBox(); // Текст файла
            file.Controls.Add(inputBoxFile);
            inputBoxFile.Dock = DockStyle.Fill;
            inputBoxFile.Location = new Point(51, 3);
            inputBoxFile.BorderStyle = BorderStyle.None;
            inputBoxFile.Font = new Font("Microsoft Sans Serif", 12F);
            inputBoxFile.SelectionChanged += new EventHandler(inputBox_SelectionChanged);
            inputBoxFile.VScroll += new EventHandler(inputBox_VScroll);
            inputBoxFile.TextChanged += new EventHandler(inputBox_TextChanged);
            inputBox = inputBoxFile;

            ReportFile reportFile = new ReportFile();
            report.Add(reportFile);
            ReportFile reportFileANTLR = new ReportFile();
            reportANTLR.Add(reportFileANTLR);

            RichTextBox LineNumberFile = new RichTextBox();
            file.Controls.Add(LineNumberFile);
            LineNumberFile.Dock = DockStyle.Left;
            LineNumberFile.BackColor = SystemColors.Window;
            LineNumberFile.Location = new Point(3, 3);
            LineNumberFile.Width = 48;
            LineNumberFile.SelectionAlignment = HorizontalAlignment.Center;
            LineNumberFile.Font = new Font("Microsoft Sans Serif", 12F);
            LineNumberFile.BorderStyle = BorderStyle.None;
            LineNumberFile.Cursor = Cursors.PanNE;
            LineNumberFile.ForeColor = SystemColors.GrayText;
            LineNumberFile.ReadOnly = true;
            LineNumberFile.ScrollBars = RichTextBoxScrollBars.None;
            LineNumberFile.MouseDown += new MouseEventHandler(LineNumber_MouseDown);
            LineNumberFile.HideSelection = true;

            UpdateLineNumbers(inputBoxFile);

            foreach (ToolStripItem button in toolStrip1.Items)
            {
                button.Enabled = true;
            }
            foreach (ToolStripMenuItem menu in menuStrip1.Items)
            {
                menu.Enabled = true;
                foreach (var dropMenu in menu.DropDownItems)
                {
                    if (dropMenu is ToolStripMenuItem)
                    {
                        ToolStripMenuItem dropMenuItem = dropMenu as ToolStripMenuItem;
                        dropMenuItem.Enabled = true;
                    }
                }
            }
            пускToolStripMenuItem.Enabled = true;

            file.DragDrop += new DragEventHandler(FormGUI_DragDrop);
            file.DragEnter += new DragEventHandler(FormGUI_DragEnter);

            return file;
        }
        private void OpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            files.TabPages.Add(NewFile(openFileDialog1.SafeFileName));
            files.SelectedTab = files.TabPages[files.TabPages.Count - 1];
            RichTextBox inputBox = files.TabPages[files.TabPages.Count - 1].GetChildAtPoint(new Point(51, 3)) as RichTextBox;
            inputBox.Text = File.ReadAllText(openFileDialog1.FileName);
        }
        private void SaveFile_Click(object sender, EventArgs e)
        {
            string filename = files.SelectedTab.Text;
            RichTextBox inputBox = files.SelectedTab.GetChildAtPoint(new Point(51, 3)) as RichTextBox;
            File.WriteAllText(filename, inputBox.Text);
        }
        private void SaveAsFile_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = files.SelectedTab.Text;
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel) return;

            string nameFile = saveFileDialog1.FileName;
            RichTextBox inputBox = files.SelectedTab.GetChildAtPoint(new Point(51, 3)) as RichTextBox;
            File.WriteAllText(nameFile, inputBox.Text);
            files.SelectedTab.Text = nameFile;
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Обновляем окна при смене файлов
        private void files_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Настраиваем вывод ошибок для каждого файла
            outputBox.Rows.Clear();

            ReportFile reportFile = report[files.SelectedIndex];
            for (int i = 0; i < reportFile.message.Count; i++)
            {
                if (reportFile.message[i] != " " && reportFile.message[i] != "")
                {
                    outputBox.Rows.Add(reportFile.path[i], reportFile.line[i], reportFile.column[i], reportFile.message[i]);
                }
            }


            // Делаем указатель на окно редактора в используемом файле
            inputBox = files.TabPages[files.SelectedIndex].GetChildAtPoint(new Point(51, 3)) as RichTextBox;
            inputBox.Focus();
        }

        // Перетаскивание файлов в приложение
        private void FormGUI_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
        private void FormGUI_DragDrop(object sender, DragEventArgs e)
        {
            this.Cursor = Cursors.Default;
            string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
            openFileDialog1.FileName = file[0];
            openFileDialog1.ShowDialog();
        }

        // Меню - правка

        private void Cancel_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Undo();
        }
        private void Return_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Redo();
        }
        private void Cut_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Cut();
        }
        private void Copy_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Copy();
        }
        private void Paste_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Paste();
        }
        private void Delete_Click(object sender, EventArgs e)
        {
            if (inputBox != null)
            {
                if (inputBox.SelectionLength > 0)
                {
                    inputBox.SelectedText = "";
                    return;
                }
                if (inputBox.SelectionStart < inputBox.Text.Length)
                {
                    inputBox.Select(inputBox.SelectionStart, 1);
                    inputBox.SelectedText = "";
                }
            }
        }
        private void SelectAll_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.SelectAll();
        }

        // Меню - Пуск

        private void Run_Click(object sender, EventArgs e)
        {
            try
            {
                outputBox.Rows.Clear();
                outputBoxParser.Rows.Clear();
                outputBoxTetrad.Rows.Clear();
                richTextBoxPOLIZ.Clear();

                scanner scannerWork = new scanner();
                List<Token> tokens = scannerWork.analyze(inputBox.Text);

                Parser syntParser = new Parser();
                List<SyntError> errorParser = syntParser.Parse(tokens);

                POLIZ pOLIZ = new POLIZ();

                ReportFile tempReport = new ReportFile();

                foreach (Token token in tokens)
                {
                    outputBox.Rows.Add(token.id, token.type, token.name, token.location);
                    tempReport.addReport(token.id.ToString(), token.type, token.name, token.location);
                }

                foreach (SyntError error in errorParser)
                {
                    outputBoxParser.Rows.Add(error.invalidFragment, error.location, error.description);
                }
                if (errorParser.Count == 1 && errorParser[0].invalidFragment == "Успешно" && errorParser[0].location == "" && errorParser[0].description == "Синтаксический анализ завершен без ошибок")
                {
                    List<Tetrad> tetrads = pOLIZ.calculate(tokens);

                    foreach (Tetrad tetrad in tetrads)
                    {
                        outputBoxTetrad.Rows.Add(tetrad.operation, tetrad.argument1, tetrad.argument2, tetrad.result);
                    }

                    richTextBoxPOLIZ.Text = pOLIZ.getResultPOLIZ();
                }
                else outputBoxParser.Rows.Add("Количество ошибок:", errorParser.Count, "");


                int indexFile = files.SelectedIndex;
                report.RemoveAt(indexFile);
                report.Insert(indexFile, tempReport);

                this.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Меню - Справка

        private void Help_Click(object sender, EventArgs e)
        {
            Form help = new Form();
            help.Size = new System.Drawing.Size(500, 500);
            help.Text = "Справочная служба";
            help.StartPosition = FormStartPosition.CenterScreen;
            RichTextBox info = new RichTextBox();
            info.ReadOnly = true;
            info.Font = statusFont.Font;
            info.Size = new System.Drawing.Size(450, 425);
            info.Location = new Point(25, 15);
            info.Text = "Меню приложения:\nФайл:\n - Создать файл - Создается новый файл и открывается в приложение для работы с ним. (Ctrl+N)\n" +
                " - Сохранить - Сохраняет текущий рабочий файл в приложении без указания пути до файла. (Ctrl+S)\n" +
                " - Cохранить как - Открывается диалоговое окно для указания расположения, где сохраниться текущий рабочий файл. (Ctrl+Shift+S)\n" +
                " - Выход - Совершается закрытие приложения. (Alt+F4)\n\nПравка:\n" +
                " - Отмена - Отменяет совершенное действие. (Ctrl+Z)\n" +
                " - Возврат - Возращает раннее отмененное действие. (Ctrl+Y)\n" +
                " - Вырезать - Стирает выделенный текст и сохраняет в буфере обмене для вставки. (Ctrl+X)\n" +
                " - Копировать - выделенный текст сохраняет в буфере обмене. (Ctrl+C)\n" +
                " - Вставить - Текст из буфере обмене напишеться в окне редактирования. (Ctrl+V)\n" +
                " - Удалить - Удаляет весь текст из окна редактирования. (Delete)\n" +
                " - Выделить все - Выделяет весь текст из окна редактирования. (Ctrl+A)\n\nПуск:\n" +
                " - Выводиться сообщение о том, что позже будет реализован пуск. (F5)\n\nСправка:\n" +
                " - Вызов справки - Появляется окно об всех функциях в приложении. (Ctrl+F1)\n" +
                " - О программе - Появляется окно с информации об окне. (Ctrl+F2)\n\nЛокализация:\n" +
                " - Русский - Меняет текст на русский язык. \n" +
                " - Китайский - Меняет текст на китайский язык. \n\nРазмер текста:\n" +
                " - прибавить - Увеличивает размер текста во всем приложении. (Ctrl+ +)\n" +
                " - убавить - Увеличивает размер текста во всем приложении. (Ctrl+ -)";
            help.Controls.Add(info);
            help.ShowDialog();
        }
        private void AboutProgram_Click(object sender, EventArgs e)
        {
            Form about = new Form();
            about.Text = "О программе";
            about.StartPosition = FormStartPosition.CenterScreen;
            RichTextBox form = new RichTextBox();
            form.ReadOnly = true;
            form.Multiline = true;
            form.Dock = DockStyle.Fill;
            form.Text = "Программа - пользовательский графический интерфейс для компилятора!\n" +
                "Выполнил: Тарбаев Даба-Цырен АП-327" +
                "\nПроверил: Антонянц Егор Николаевич асс. каф. АСУ.";
            about.Controls.Add(form);
            about.Show();
        }

        // Меню - Локализация

        private void RusLg_Click(object sender, EventArgs e)
        {
            swap_lang(0);
        }

        private void ChinaLg(object sender, EventArgs e)
        {
            swap_lang(1);
        }
        private void swap_lang(int key)
        {
            if (key == 0) файлToolStripMenuItem.Text = "Файл";
            else файлToolStripMenuItem.Text = "文件";

            if (key == 0) правкаToolStripMenuItem.Text = "Правка";
            else правкаToolStripMenuItem.Text = "编辑";

            if (key == 0) текстToolStripMenuItem.Text = "Текст";
            else текстToolStripMenuItem.Text = "文本";

            if (key == 0) пускToolStripMenuItem.Text = "Пуск";
            else пускToolStripMenuItem.Text = "开始";

            if (key == 0) справкаToolStripMenuItem.Text = "Справка";
            else справкаToolStripMenuItem.Text = "参考";

            if (key == 0) размерТекстаToolStripMenuItem.Text = "Размер текста";
            else размерТекстаToolStripMenuItem.Text = "文字大小";

            if (key == 0) локалиToolStripMenuItem.Text = "Язык";
            else локалиToolStripMenuItem.Text = "本地化";

            if (key == 0) созданиеToolStripMenuItem.Text = "Создать";
            else созданиеToolStripMenuItem.Text = "创造";

            if (key == 0) открытиеToolStripMenuItem.Text = "Открыть";
            else открытиеToolStripMenuItem.Text = "打开";

            if (key == 0) сохранениеToolStripMenuItem.Text = "Сохранить";
            else сохранениеToolStripMenuItem.Text = "节省";

            if (key == 0) сохранениеКакToolStripMenuItem.Text = "Сохранить как";
            else сохранениеКакToolStripMenuItem.Text = "另存为";

            if (key == 0) выходToolStripMenuItem.Text = "Выход";
            else выходToolStripMenuItem.Text = "出口";

            if (key == 0) MenuItemCancel.Text = "Отменить";
            else MenuItemCancel.Text = "取消";

            if (key == 0) MenuItemReturn.Text = "Повторить";
            else MenuItemReturn.Text = "重复";

            if (key == 0) MenuItemCut.Text = "Вырезать";
            else MenuItemCut.Text = "切";

            if (key == 0) MenuItemCopy.Text = "Копировать";
            else MenuItemCopy.Text = "复制";

            if (key == 0) MenuItemPaste.Text = "Вставить";
            else MenuItemPaste.Text = "插入";

            if (key == 0) MenuItemDelete.Text = "Удалить";
            else MenuItemDelete.Text = "删除";

            if (key == 0) русскийToolStripMenuItem.Text = "Русский";
            else русскийToolStripMenuItem.Text = "俄语";

            if (key == 0) китайскийToolStripMenuItem.Text = "Китайский";
            else китайскийToolStripMenuItem.Text = "中文";

            if (key == 0) увеличитьToolStripMenuItem.Text = "Увеличить";
            else увеличитьToolStripMenuItem.Text = "增加";

            if (key == 0) уменьшитьToolStripMenuItem.Text = "Уменьшить";
            else уменьшитьToolStripMenuItem.Text = "减少";

            if (key == 0) выделениеВсегоТекстаToolStripMenuItem.Text = "Выделить все";
            else выделениеВсегоТекстаToolStripMenuItem.Text = "选择全部";

            if (key == 0) постановкаЗадачиToolStripMenuItem.Text = "Постановка задачи";
            else постановкаЗадачиToolStripMenuItem.Text = "问题陈述";

            if (key == 0) грамматикаЯзыкаToolStripMenuItem.Text = "Грамматика";
            else грамматикаЯзыкаToolStripMenuItem.Text = "语法";

            if (key == 0) классификацияГрамматикиToolStripMenuItem.Text = "Классификация грамматики";
            else классификацияГрамматикиToolStripMenuItem.Text = "语法分类";

            if (key == 0) методАнализаToolStripMenuItem.Text = "Метод анализа";
            else методАнализаToolStripMenuItem.Text = "分析方法";

            if (key == 0) тестовыйПримерToolStripMenuItem.Text = "Тестовый пример";
            else тестовыйПримерToolStripMenuItem.Text = "测试用例";

            if (key == 0) списокЛитературыToolStripMenuItem.Text = "Список литературы";
            else списокЛитературыToolStripMenuItem.Text = "参考";

            if (key == 0) исходныйКодПрограммыToolStripMenuItem.Text = "Исходный код программы";
            else исходныйКодПрограммыToolStripMenuItem.Text = "程序源码";

            if (key == 0) вызовСправкиToolStripMenuItem.Text = "Вызов справки";
            else вызовСправкиToolStripMenuItem.Text = "寻求帮助";

            if (key == 0) оПрограммеToolStripMenuItem.Text = "О программе";
            else оПрограммеToolStripMenuItem.Text = "关于该计划";

            if (key == 0) outputBox.Columns[0].HeaderText = "Путь файла";
            else outputBox.Columns[0].HeaderText = "文件路径";

            if (key == 0) outputBox.Columns[1].HeaderText = "Строка";
            else outputBox.Columns[1].HeaderText = "线";

            if (key == 0) outputBox.Columns[2].HeaderText = "Колонка";
            else outputBox.Columns[2].HeaderText = "柱子";

            if (key == 0) outputBox.Columns[3].HeaderText = "Сообщение";
            else outputBox.Columns[3].HeaderText = "信息";

            if (key == 0) this.Text = "Языковой процессор";
            else this.Text = "语言处理器";

            if (key == 0) sec = "сек";
            else sec = "第二";

            if (key == 0) time = "Время работы приложения: ";
            else time = "申请开放时间";

            int hours = totalSec / 3600;
            int minutes = (totalSec % 3600) / 60;
            int seconds = totalSec % 60;

            string timeStatus;

            if (hours > 0) timeStatus = $"{hours}:{minutes}:{seconds}";
            else if (minutes > 0) timeStatus = $"{minutes}:{seconds}";
            else timeStatus = $"{seconds} " + sec;

            statusTimeApp.Text = time + timeStatus;
        }


        // Меню - Размер текста

        private void fontSize_Up(object sender, EventArgs e)
        {
            float size = buttonCancel.Font.Size;
            string font = buttonCancel.Font.Name;

            Size begin_size = this.Size;

            font_Change(this, size + 1, font);

            this.Size = begin_size;

            statusFont.Text = $"Шрифт: {font} {size + 1}pt";
        }
        private void fontSize_Down(object sender, EventArgs e)
        {
            float size = buttonCancel.Font.Size;
            string font = buttonCancel.Font.Name;

            font_Change(this, size - 1, font);

            statusFont.Text = $"Шрифт: {font} {size - 1}pt";
        }
        private void font_Change(Control control, float size, string font)
        {
            control.Font = new Font(font, size);
            foreach (Control sub in control.Controls)
            {
                font_Change(sub, size, font);
            }
        }

        // Строка состояния приложения
        private int totalSec = 0;
        private string sec = "сек";
        private string time = "Время работы приложения: ";
        private void timerApp_Tick(object sender, EventArgs e)
        {
            totalSec++;

            int hours = totalSec / 3600;
            int minutes = (totalSec % 3600) / 60;
            int seconds = totalSec % 60;

            string timeStatus;

            if (hours > 0) timeStatus = $"{hours}:{minutes}:{seconds}";
            else if (minutes > 0) timeStatus = $"{minutes}:{seconds}";
            else timeStatus = $"{seconds} " + sec;

            statusTimeApp.Text = time + timeStatus;
        }

        // Нумерация строк для окна редактирования

        private void UpdateLineNumbers(object sender)
        {
            RichTextBox inputBox = sender as RichTextBox;
            TabPage file = inputBox.Parent as TabPage;
            RichTextBox LineNumber = file.GetChildAtPoint(new Point(3, 3)) as RichTextBox;

            Point pt = new Point(0, 0);
            int First_Index = inputBox.GetCharIndexFromPosition(pt);
            int First_Line = inputBox.GetLineFromCharIndex(First_Index);

            pt.X = ClientRectangle.Width;
            pt.Y = ClientRectangle.Height;

            int Last_Index = inputBox.GetCharIndexFromPosition(pt);
            int Last_Line = inputBox.GetLineFromCharIndex(Last_Index);

            LineNumber.SelectionAlignment = HorizontalAlignment.Center;
            LineNumber.Text = "";
            for (int i = First_Line; i <= Last_Line; i++)
            {
                LineNumber.Text += i + 1 + "\n";
            }
        }
        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            if (curr_row != inputBox.GetLineFromCharIndex(inputBox.GetFirstCharIndexOfCurrentLine()) || curr_row == 0)
            {
                UpdateLineNumbers(sender);
                curr_row = inputBox.GetLineFromCharIndex(inputBox.GetFirstCharIndexOfCurrentLine());
            }
        }

        private void inputBox_VScroll(object sender, EventArgs e)
        {
            RichTextBox inputBox = sender as RichTextBox;
            TabPage file = inputBox.Parent as TabPage;
            RichTextBox LineNumber = file.GetChildAtPoint(new Point(3, 3)) as RichTextBox;

            LineNumber.Text = "";
            UpdateLineNumbers(sender);
            LineNumber.Invalidate();
        }
        private void inputBox_SelectionChanged(object sender, EventArgs e)
        {
            RichTextBox inputBox = sender as RichTextBox;
            Point pt = inputBox.GetPositionFromCharIndex(inputBox.SelectionStart);
            if (pt.X == 0)
            {
                UpdateLineNumbers(sender);
            }
        }

        private void LineNumber_MouseDown(object sender, MouseEventArgs e)
        {
            RichTextBox lineNumber = sender as RichTextBox;
            TabPage file = lineNumber.Parent as TabPage;
            RichTextBox inputBox = file.GetChildAtPoint(new Point(51, 3)) as RichTextBox;

            int charIndex = lineNumber.GetCharIndexFromPosition(e.Location);
            int visualLineIndex = lineNumber.GetLineFromCharIndex(charIndex);

            // Проверяем, есть ли текст в этой строке lineNumber
            if (visualLineIndex < lineNumber.Lines.Length)
            {
                string lineText = lineNumber.Lines[visualLineIndex].Trim();

                if (int.TryParse(lineText, out int lineNumberValue) && lineNumberValue > 0)
                {
                    int targetLine = lineNumberValue - 1;

                    if (targetLine >= 0 && targetLine < inputBox.Lines.Length)
                    {
                        int startIndex = inputBox.GetFirstCharIndexFromLine(targetLine);
                        int lineLength = inputBox.Lines[targetLine].Length;

                        inputBox.Select(startIndex, lineLength);

                        inputBox.Focus();
                    }
                }
            }
        }
        // Текущая строка
        private int curr_row = 0;
        // Текущее слово
        private string word = "";
        private void backfillkeywords(object sender, int pos, int len)
        {
            RichTextBox text_box = sender as RichTextBox;
            text_box.SelectionStart = pos;
            text_box.SelectionLength = len;
            text_box.SelectionBackColor = Color.Yellow;
            text_box.SelectionStart = pos + len;
            text_box.SelectionLength = 0;
            text_box.SelectionBackColor = Color.White;
        }
        // Подсветка ключевых слов
        private void inputBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            RichTextBox text_box = sender as RichTextBox;
            if (e.KeyChar == '\r' || e.KeyChar == ' ' || e.KeyChar == '\0' || e.KeyChar == (char)Keys.Back)
            {
                int curr_pos = 0;
                if (e.KeyChar == '\r') curr_row--;
                switch (word.ToLower())
                {
                    case "if":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case "else":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case "int":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case "float":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case "while":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case "for":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                }
                word = "";
            }
            else word += e.KeyChar;
        }

        private void FormGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result;
            if (files.SelectedIndex > -1)
            {
                result = MessageBox.Show("У вас есть несохраненный файл, сохранить?", "сообщение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    сохранениеКакToolStripMenuItem.PerformClick();
                }
            }
        }
        public class ReportFile
        {
            public List<string> path = new List<string>();
            public List<string> line = new List<string>();
            public List<string> column = new List<string>();
            public List<string> message = new List<string>();
            public ReportFile(string path = "", string line = "0", string column = "0", string message = "")
            {
                addReport(path, line, column, message);
            }
            public void addReport(string path = "", string line = "0", string column = "0", string message = "")
            {
                this.path.Add(path);
                this.line.Add(line);
                this.column.Add(column);
                this.message.Add(message);
            }
        }

        private void outputBox_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            inputBox.Focus();
            if (e.RowIndex == -1) return;
            string position = outputBox.Rows[e.RowIndex].Cells[3].Value.ToString();
            string location = position.Split(' ')[1];
            string line = location.Split(',')[0];
            int numberLine = Convert.ToInt32(line);
            string inLine = position.Split(' ')[2];
            int positioninstr = Convert.ToInt32(inLine.Split('-')[0]);
            inputBox.Select(inputBox.GetFirstCharIndexFromLine(numberLine - 1) + positioninstr - 1, 1);
        }

        private void outputBoxParser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            inputBox.Focus();
            if (e.RowIndex == -1) return;
            string position = outputBoxParser.Rows[e.RowIndex].Cells[1].Value.ToString();
            if (position == "позиция неизвестна")
            {
                inputBox.Select(inputBox.Text.Length, 1);
                return;
            }
            else if (outputBoxParser.Rows[e.RowIndex].Cells[0].Value.ToString() == "Количество ошибок:") return;
            else if (position == "") return;
            string location = position.Split(' ')[1];
            string line = location.Split(',')[0];
            int numberLine = Convert.ToInt32(line);
            string inLine = position.Split(' ')[2];
            int positioninstr = Convert.ToInt32(inLine.Split('-')[0]);
            inputBox.Select(inputBox.GetFirstCharIndexFromLine(numberLine - 1) + positioninstr - 1, 1);
        }

        private void outputBoxSemError_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //inputBox.Focus();
            //if (e.RowIndex == -1) return;
            //string position = outputBoxTetrad.Rows[e.RowIndex].Cells[1].Value.ToString();
            //string location = position.Split(' ')[1];
            //string line = location.Split(',')[0];
            //int numberLine = Convert.ToInt32(line);
            //string inLine = position.Split(' ')[2];
            //int positioninstr = Convert.ToInt32(inLine.Split('-')[0]);
            //inputBox.Select(inputBox.GetFirstCharIndexFromLine(numberLine - 1) + positioninstr - 1, 1);
        }

        // Меню - Текст

        private void постановкаЗадачиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = "Постановка задачи";
            form.Size = new Size(700, 400);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font("Times New Roman", 14);
            textBox.Text = @"   Постановка задачи

    Комплексное число — это число, которое состоит из реальной части числа и мнимой части числа. Комплексное число z обычно записывается в форме z = x + yi, где x и y являются реальными числами, и i — мнимая единица, которая имеет свойство i² = -1.

    Для объявления комплексного числа с инициализацией на языке C# используется ключевое слово Complex.

    Формат записи: ""Complex имя_переменной = new Complex(число1, число2);""

    Примеры верных записей:
    1) ""Complex c1 = new Complex(1.2, 6.0);""
    2) ""Complex comp1 = new Complex(146, -6);""
    3) ""Complex number = new Complex(-1.252, 180);""";

            form.Controls.Add(textBox);
            form.Show();
        }

        private void грамматикаЯзыкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = "Разработанная грамматика";
            form.Size = new Size(800, 600);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font("Times New Roman", 14);
            textBox.Text = @"   Разработанная грамматика
    
    Определим грамматику объявления комплексного числа с инициализацией на языке C# G[<START>] в нотации Хомского с продукциями P:

    G[<START>]:
    1)  <START> → 'Complex'<ID>
    2)  <ID> → letter<IDREM>
    3)  <IDREM> → letter<IDREM> | digit<IDREM> | '='<NEW>
    4)  <NEW> → 'new'<TYPE>
    5)  <TYPE> → 'Complex'<OPEN_PAREN>
    6)  <OPEN_PAREN> → '('<SIGN>
    7)  <SIGN> → '+'<DIGIT_REAL> | '-'<DIGIT_REAL> | digit<REAL>
    8)  <DIGIT_REAL> → digit<REAL>
    9)  <REAL> → digit<REAL> | '.'<REAL_DOT> | ','<IMAG_SIGN>
    10) <REAL_DOT> → digit<REAL_FRACTION>
    11) <REAL_FRACTION> → digit<REAL_FRACTION> | ','<IMAG_SIGN>
    12) <IMAG_SIGN> → '+'<DIGIT_IMAG> | '-'<DIGIT_IMAG> | digit<IMAG>
    13) <DIGIT_IMAG> → digit<IMAG>
    14) <IMAG> → digit<IMAG> | '.'<IMAG_DOT> | ')'<END>
    15) <IMAG_DOT> → digit<IMAG_FRACTION>
    16) <IMAG_FRACTION> → digit<IMAG_FRACTION> | ')'<END>
    17) <END> → ';'

    letter → 'a' | 'b' | ... | 'z' | 'A' | 'B' | ... | 'Z'
    digit → '0' | '1' | ... | '9'

    Следуя введенному формальному определению грамматики, представим G[<START>] ее составляющими:

    Z = <START>

    VT = {a, b, ..., z, A, B, ..., Z, 0, 1, ..., 9, +, -, ., ,, (, ), ;}

    VN = {<START>, <ID>, <IDREM>, <NEW>, <TYPE>, <OPEN_PAREN>, <SIGN>, 
        <DIGIT_REAL>, <REAL>, <REAL_DOT>, <REAL_FRACTION>, <IMAG_SIGN>, 
        <DIGIT_IMAG>, <IMAG>, <IMAG_DOT>, <IMAG_FRACTION>, <END>}";

            form.Controls.Add(textBox);
            form.Show();
        }

        private void классификацияГрамматикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = "Классификация грамматики";
            form.Size = new Size(800, 600);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font("Times New Roman", 14);
            textBox.Text = @"   Классификация грамматики

    Согласно классификации Хомского, грамматика G[<START>] является автоматной.

    Правила (1)-(17) относятся к классу праворекурсивных продукций (A → aB | a | ε):

    1)  <START> → 'Complex'<ID>
    2)  <ID> → letter<IDREM>
    3)  <IDREM> → letter<IDREM> | digit<IDREM> | '='<NEW>
    4)  <NEW> → 'new'<TYPE>
    5)  <TYPE> → 'Complex'<OPEN_PAREN>
    6)  <OPEN_PAREN> → '('<SIGN>
    7)  <SIGN> → '+'<DIGIT_REAL> | '-'<DIGIT_REAL> | digit<REAL>
    8)  <DIGIT_REAL> → digit<REAL>
    9)  <REAL> → digit<REAL> | '.'<REAL_DOT> | ','<IMAG_SIGN>
    10) <REAL_DOT> → digit<REAL_FRACTION>
    11) <REAL_FRACTION> → digit<REAL_FRACTION> | ','<IMAG_SIGN>
    12) <IMAG_SIGN> → '+'<DIGIT_IMAG> | '-'<DIGIT_IMAG> | digit<IMAG>
    13) <DIGIT_IMAG> → digit<IMAG>
    14) <IMAG> → digit<IMAG> | '.'<IMAG_DOT> | ')'<END>
    15) <IMAG_DOT> → digit<IMAG_FRACTION>
    16) <IMAG_FRACTION> → digit<IMAG_FRACTION> | ')'<END>
    17) <END> → ';'

    Отметим, что правила должны быть либо только леворекурсивными, либо только праворекурсивными. Комбинация тех и других не допускается. Данная грамматика не содержит леворекурсивных продукций, все рекурсивные правила являются праворекурсивными, следовательно, грамматика является автоматной (праволинейной).";

            form.Controls.Add(textBox);
            form.Show();
        }

        private void методАнализаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = "Метод анализа";
            form.Size = new Size(800, 600);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.Font = new Font("Times New Roman", 14);
            // Текст с выравниванием по центру
            textBox.SelectionFont = new Font("Times New Roman", 14, FontStyle.Regular);
            textBox.SelectionAlignment = HorizontalAlignment.Left;
            textBox.AppendText("Метод анализа\n\n");
            textBox.AppendText("На рисунке 1 представлена посимвольная декомпозиция объявления комплексного числа " +
                              "с генерацией соответствующего символического кода символов <ID> и т.д., и литеры «+», «–» и др. " +
                              "Непомеченные дуги на диаграмме соответствуют состоянию ERROR (отсутствие данного символа в " +
                              "словаре грамматики) либо выходу из обработки очередного символа и переходу на старт обработки следующего.\n\n");

            textBox.SelectionAlignment = HorizontalAlignment.Center;
            Clipboard.SetImage(Properties.Resources.diagram_states);
            textBox.Paste();
            textBox.AppendText("\nРисунок 1 – Диаграмма состояний\n");
            textBox.AppendText("\n\n");

            textBox.SelectionAlignment = HorizontalAlignment.Left;
            textBox.AppendText("Грамматика G[<START>] является автоматной.\n");
            textBox.AppendText("Правила (1) – (17) для G[<START>] реализованы на графе (см. рисунок 2).\n");
            textBox.AppendText("Сплошные стрелки на графе характеризуют синтаксически верный разбор. " +
                              "Состояние 18 символизирует успешное завершение разбора.\n\n");

            textBox.SelectionAlignment = HorizontalAlignment.Center;
            Clipboard.SetImage(Properties.Resources.graph_grammar);
            textBox.Paste();
            textBox.AppendText("\nРисунок 2 – Граф G[<START>]\n");

            textBox.ReadOnly = true;
            textBox.SelectionStart = 0;
            textBox.SelectionLength = 0;

            form.Controls.Add(textBox);
            form.Show();
        }

        private void тестовыйПримерToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = "Метод анализа";
            form.Size = new Size(600, 500);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;

            textBox.Font = new Font("Times New Roman", 14);
            // Текст с выравниванием по центру

            textBox.SelectionFont = new Font("Times New Roman", 14, FontStyle.Regular);
            textBox.SelectionAlignment = HorizontalAlignment.Left;
            textBox.AppendText("Тестовые примеры\n\n");
            textBox.AppendText("На рисунках представлены тестовые примеры запуска разработанного лексического анализатора для объявления комплексного числа с инициализацией на языке C#.\n");
            textBox.AppendText("1) “Complex c1 = new Complex(1.2, 6.0);”\n");
            Clipboard.SetImage(Properties.Resources.test1);
            textBox.Paste();
            textBox.AppendText("\n2) “Complex comp1 = new Complex(146, -6);”\n");
            Clipboard.SetImage(Properties.Resources.test2);
            textBox.Paste();
            textBox.AppendText("\n3) “Complexname = new Com@plex(10, -6.76);”\n");
            Clipboard.SetImage(Properties.Resources.test3);
            textBox.Paste();
            textBox.AppendText("\n4) “Complex idddd === # new Complex((32,32);”\n");
            Clipboard.SetImage(Properties.Resources.test4);
            textBox.Paste();
            textBox.AppendText("\n5) “Complexxx +21name=new Complex(#,3);”\n");
            Clipboard.SetImage(Properties.Resources.test5);
            textBox.Paste();

            textBox.ReadOnly = true;
            textBox.SelectionStart = 0;
            textBox.SelectionLength = 0;

            form.Controls.Add(textBox);
            form.Show();
        }

        private void списокЛитературыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = "Список использованных источников";
            form.Size = new Size(800, 350);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font("Times New Roman", 14, FontStyle.Regular);
            textBox.Text = @"Список использованных источников

1. Шорников Ю. В. Теория языков программирования: проектирование и реализация : учебное пособие / Ю. В. Шорников. — Новосибирск : Изд-во НГТУ, 2022. — 290 с.

2. Gries D. Designing Compilers for Digital Computers. New York, Jhon Wiley, 1971. 493 p.

3. Теория формальных языков и компиляторов [Электронный ресурс] / Электрон. дан. URL: https://dispace.edu.nstu.ru/didesk/course/show/8594, свободный. Яз.рус. (дата обращения 14.04.2026).";

            form.Controls.Add(textBox);
            form.Show();
        }

        private void исходныйКодПрограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = "Исходный код программы";
            form.Size = new Size(700, 500);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font("Consolas", 9);
            textBox.WordWrap = false;
            textBox.ScrollBars = RichTextBoxScrollBars.Both;
            textBox.Text = @"Исходный код программы (scanner.cs)
using System;
using System.Collections.Generic;

namespace Scanner
{
    public class Token
    {
        public readonly int id;
        public readonly string type;
        public readonly string name;
        public readonly string location;
        public Token(int id, string type, string name, string location)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.location = location;
        }
    }
    public class scanner
    {
        private string text;
        private char liter;
        private int currentPosition = 0;
        private int positionLine = 0;
        private int currentLine = 1;
        private string buffer = """";
        private List<Token> tokens = new List<Token>();
        public List<Token> analyze(string inputText)
        {
            text = inputText;

            getNext();

            while (currentPosition <= text.Length)
            {
                if (char.IsLetter(liter))
                {
                    buffer += liter;
                    while (char.IsLetterOrDigit(liter = getChar()))
                    {
                        buffer += liter;
                    }
                    switch (buffer)
                    {
                        case ""Complex"":
                            addToken(1, ""Ключевое слово Complex"", buffer);
                            break;
                        case ""new"":
                            addToken(2, ""Ключевое слово new"", buffer);
                            break;
                        default:
                            addToken(3, ""Идентификатор"", buffer);
                            break;
                    }
                    buffer = """";
                }
                else if (char.IsDigit(liter))
                {
                    buffer += liter;
                    while (char.IsDigit(liter = getChar()))
                    {
                        buffer += liter;
                    }
                    if (liter == '.')
                    {
                        buffer += liter;
                        while (char.IsDigit(liter = getChar()))
                        {
                            buffer += liter;
                        }
                        addToken(11, ""Вещественное число"", buffer);
                    }
                    else
                    {
                        addToken(10, ""Целое без знака"", buffer);
                    }
                    buffer = """";
                }
                else
                {
                    switch (liter)
                    {
                        case '\0':
                            getNext();
                            break;
                        case '\n':
                            positionLine = 0;
                            currentLine++;
                            getNext();
                            break;
                        case '=':
                            buffer += liter;
                            while ((liter = getChar()) == '=')
                            {
                                buffer += liter;
                            }
                            addToken(4, ""Оператор присваивания"", buffer);
                            buffer = """";
                            break;
                        case ' ':
                            buffer += liter;
                            while ((liter = getChar()) == ' ')
                            {
                                buffer += liter;
                            }
                            addToken(5, ""Разделитель"", buffer);
                            buffer = """";
                            break;
                        case '(':
                            buffer += liter;
                            while ((liter = getChar()) == '(')
                            {
                                buffer += liter;
                            }
                            addToken(6, ""Оператор конструктора"", buffer);
                            buffer = """";
                            break;
                        case ')':
                            buffer += liter;
                            while ((liter = getChar()) == ')')
                            {
                                buffer += liter;
                            }
                            addToken(7, ""Оператор конструктора"", buffer);
                            buffer = """";
                            break;
                        case '-':
                            buffer += liter;
                            while ((liter = getChar()) == '-')
                            {
                                buffer += liter;
                            }
                            addToken(8, ""Знак минуса"", buffer);
                            buffer = """";
                            break;
                        case '+':
                            buffer += liter;
                            while ((liter = getChar()) == '+')
                            {
                                buffer += liter;
                            }
                            addToken(9, ""Знак плюса"", buffer);
                            buffer = """";
                            break;
                        case ',':
                            buffer += liter;
                            while ((liter = getChar()) == ',')
                            {
                                buffer += liter;
                            }
                            addToken(12, ""Оператор перечисления"", buffer);
                            buffer = """";
                            break;
                        case ';':
                            buffer += liter;
                            while ((liter = getChar()) == ';')
                            {
                                buffer += liter;
                            }
                            addToken(13, ""Оператор заверешения"", buffer);
                            buffer = """";
                            break;
                        default:
                            buffer += liter;
                            while (!(char.IsLetterOrDigit(liter = getChar()) || liter == '\0' || liter == '\n' || liter == ' '
                                || liter == '(' || liter == ')' || liter == '=' || liter == '-' || liter == '+' || liter == ',' || liter == ';'))
                            {
                                buffer += liter;
                            }
                            addToken(-1, ""Недопустимый символ"", buffer);
                            buffer = """";
                            break;
                    }
                }
            }

            return tokens;
        }
        private char getChar()
        {
            try
            {
                if (currentPosition >= text.Length)
                {
                    currentPosition++;
                    positionLine++;
                    return '\0';
                }
                char liter1 = text[currentPosition];
                currentPosition++;
                positionLine++;
                return liter1;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception(""В конце строки не обнаружено ;"");
            }
        }
        private void getNext()
        {
            liter = getChar();
        }
        private void addToken(int id, string type, string name)
        {
            int Length = name.Length;
            int leng = positionLine - Length;
            string loc = $""строка {currentLine}, {leng}-{positionLine - 1}"";
            tokens.Add(new Token(id, type, name, loc));
        }
    }
}

FormGUI.cs

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;


namespace Scanner
{
    public partial class FormGUI : Form
    {
        // Указатель на окно редактирования
        private RichTextBox inputBox;
        // Лист всех ошибок из всех файлов
        private List<ReportFile> report = new List<ReportFile>();
        private List<ReportFile> reportANTLR = new List<ReportFile>();

        public FormGUI()
        {
            InitializeComponent();
        }

        // Меню - Файл

        private void CreatFile_Click(object sender, EventArgs e)
        {
            files.TabPages.Add(NewFile(""Новый документ"" + (files.TabCount + 1).ToString()));
        }
        private TabPage NewFile(string name)
        {
            TabPage file = new TabPage(name);
            file.BorderStyle = BorderStyle.FixedSingle;

            RichTextBox inputBoxFile = new RichTextBox(); // Текст файла
            file.Controls.Add(inputBoxFile);
            inputBoxFile.Dock = DockStyle.Fill;
            inputBoxFile.Location = new Point(51, 3);
            inputBoxFile.BorderStyle = BorderStyle.None;
            inputBoxFile.Font = new Font(""Microsoft Sans Serif"", 12F);
            inputBoxFile.SelectionChanged += new EventHandler(inputBox_SelectionChanged);
            inputBoxFile.VScroll += new EventHandler(inputBox_VScroll);
            inputBoxFile.TextChanged += new EventHandler(inputBox_TextChanged);
            inputBoxFile.KeyPress += new KeyPressEventHandler(inputBox_KeyPress);
            //inputBoxFile.KeyDown += new KeyEventHandler(inputBox_KeyDown);
            inputBox = inputBoxFile;

            ReportFile reportFile = new ReportFile();
            report.Add(reportFile);
            ReportFile reportFileANTLR = new ReportFile();
            reportANTLR.Add(reportFileANTLR);

            RichTextBox LineNumberFile = new RichTextBox();
            file.Controls.Add(LineNumberFile);
            LineNumberFile.Dock = DockStyle.Left;
            LineNumberFile.BackColor = SystemColors.Window;
            LineNumberFile.Location = new Point(3, 3);
            LineNumberFile.Width = 48;
            LineNumberFile.SelectionAlignment = HorizontalAlignment.Center;
            LineNumberFile.Font = new Font(""Microsoft Sans Serif"", 12F);
            LineNumberFile.BorderStyle = BorderStyle.None;
            LineNumberFile.Cursor = Cursors.PanNE;
            LineNumberFile.ForeColor = SystemColors.GrayText;
            LineNumberFile.ReadOnly = true;
            LineNumberFile.ScrollBars = RichTextBoxScrollBars.None;
            LineNumberFile.MouseDown += new MouseEventHandler(LineNumber_MouseDown);
            LineNumberFile.HideSelection = true;

            UpdateLineNumbers(inputBoxFile);

            foreach (ToolStripItem button in toolStrip1.Items)
            {
                button.Enabled = true;
            }
            foreach (ToolStripMenuItem menu in menuStrip1.Items)
            {
                menu.Enabled = true;
                foreach (var dropMenu in menu.DropDownItems)
                {
                    if (dropMenu is ToolStripMenuItem)
                    {
                        ToolStripMenuItem dropMenuItem = dropMenu as ToolStripMenuItem;
                        dropMenuItem.Enabled = true;
                    }
                }
            }
            пускToolStripMenuItem.Enabled = true;

            file.DragDrop += new DragEventHandler(FormGUI_DragDrop);
            file.DragEnter += new DragEventHandler(FormGUI_DragEnter);

            return file;
        }
        private void OpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            files.TabPages.Add(NewFile(openFileDialog1.SafeFileName));
            files.SelectedTab = files.TabPages[files.TabPages.Count - 1];
            RichTextBox inputBox = files.TabPages[files.TabPages.Count - 1].GetChildAtPoint(new Point(51, 3)) as RichTextBox;
            inputBox.Text = File.ReadAllText(openFileDialog1.FileName);
        }
        private void SaveFile_Click(object sender, EventArgs e)
        {
            string filename = files.SelectedTab.Text;
            RichTextBox inputBox = files.SelectedTab.GetChildAtPoint(new Point(51, 3)) as RichTextBox;
            File.WriteAllText(filename, inputBox.Text);
        }
        private void SaveAsFile_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = files.SelectedTab.Text;
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel) return;

            string nameFile = saveFileDialog1.FileName;
            RichTextBox inputBox = files.SelectedTab.GetChildAtPoint(new Point(51, 3)) as RichTextBox;
            File.WriteAllText(nameFile, inputBox.Text);
            files.SelectedTab.Text = nameFile;
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Обновляем окна при смене файлов
        private void files_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Настраиваем вывод ошибок для каждого файла
            outputBox.Rows.Clear();

            ReportFile reportFile = report[files.SelectedIndex];
            for (int i = 0; i < reportFile.message.Count; i++)
            {
                if (reportFile.message[i] != "" "" && reportFile.message[i] != """")
                {
                    outputBox.Rows.Add(reportFile.path[i], reportFile.line[i], reportFile.column[i], reportFile.message[i]);
                }
            }

            ReportFile reportFileANTLR = reportANTLR[files.SelectedIndex];
            for (int i = 0; i < reportFileANTLR.message.Count; i++)
            {
                if (reportFileANTLR.message[i] != "" "" && reportFileANTLR.message[i] != """")
                {
                    outputBoxANTLR.Rows.Add(reportFileANTLR.path[i], reportFileANTLR.line[i], reportFileANTLR.column[i], reportFileANTLR.message[i]);
                }
            }

            // Делаем указатель на окно редактора в используемом файле
            inputBox = files.TabPages[files.SelectedIndex].GetChildAtPoint(new Point(51, 3)) as RichTextBox;
            inputBox.Focus();
        }

        // Перетаскивание файлов в приложение
        private void FormGUI_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
        private void FormGUI_DragDrop(object sender, DragEventArgs e)
        {
            this.Cursor = Cursors.Default;
            string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
            openFileDialog1.FileName = file[0];
            openFileDialog1.ShowDialog();
        }

        // Меню - правка

        private void Cancel_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Undo();
        }
        private void Return_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Redo();
        }
        private void Cut_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Cut();
        }
        private void Copy_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Copy();
        }
        private void Paste_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.Paste();
        }
        private void Delete_Click(object sender, EventArgs e)
        {
            if (inputBox != null)
            {
                if (inputBox.SelectionLength > 0)
                {
                    inputBox.SelectedText = """";
                    return;
                }
                if (inputBox.SelectionStart < inputBox.Text.Length)
                {
                    inputBox.Select(inputBox.SelectionStart, 1);
                    inputBox.SelectedText = """";
                }
            }
        }
        private void SelectAll_Click(object sender, EventArgs e)
        {
            if (inputBox != null) inputBox.SelectAll();
        }

        // Меню - Текст

        private void textWork_Click(object sender, EventArgs e)
        {
            MessageBox.Show(""Меню «Текст» будет реализовано позже. При вызове команд этого меню должны открываться окна с соответствующей информацией. "");
        }

        // Меню - Пуск

        private void Run_Click(object sender, EventArgs e)
        {
            try
            {
                outputBox.Rows.Clear();

                scanner scannerWork = new scanner();
                List<Token> tokens = scannerWork.analyze(inputBox.Text);

                ReportFile tempReport = new ReportFile();
                
                foreach (Token token in tokens)
                {
                    outputBox.Rows.Add(token.id, token.type, token.name, token.location);
                    tempReport.addReport(token.id.ToString(), token.type, token.name, token.location);
                    if (token.id == -1) break;
                }

                outputBoxANTLR.Rows.Clear();
                //Создание собственного слушателя синтаксических ошибок
                ErrorListener errors = new ErrorListener();
                //Создание собственного слушателя лексических ошибок
                ErrorLexerListener errorsLexer = new ErrorLexerListener();
                // Определение символьного потока, parsedText – исходный текст
                ICharStream stream = CharStreams.fromString(inputBox.Text);
                // Создание лексера на потоке stream
                ComplexLexer lexer = new ComplexLexer(stream);
                //Добавление своего слушателя лексических ошибок
                lexer.AddErrorListener(errorsLexer);
                //Создание потока токенов на основе лексера
                ITokenStream tokensANTLR = new CommonTokenStream(lexer);
                // Создание парсера
                ComplexParser parser = new ComplexParser(tokensANTLR);
                parser.BuildParseTree = true;
                // Удаление стандартных слушателей
                parser.RemoveErrorListeners();
                //Добавление своего слушателя синтаксических ошибок
                parser.AddErrorListener(errors);
                IParseTree tree = parser.prog();
                ReportFile tempReportANTLR = new ReportFile();

                for (int i = 0; i < tokensANTLR.Size; i++)
                {
                    IToken token = tokensANTLR.Get(i);
                    outputBoxANTLR.Rows.Add(token.Type, TypeNameTokenANTLR(token.Type), token.Text, $""строка {token.Line}, {token.Column + 1}-{token.Column + token.Text.Length }"");
                    tempReportANTLR.addReport(token.Type.ToString(), TypeNameTokenANTLR(token.Type), token.Text, $""строка {token.Line}, {token.Column + 1}-{token.Column + token.Text.Length + 1}"");
                    if (token.Type == 16) break;
                }

                int indexFile = files.SelectedIndex;
                report.RemoveAt(indexFile);
                report.Insert(indexFile, tempReport);

                reportANTLR.RemoveAt(indexFile);
                reportANTLR.Insert(indexFile, tempReportANTLR);

                this.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string TypeNameTokenANTLR(int cod)
        {
            switch (cod)
            {
                case 1: return ""Ключевое слово Complex"";
                case 2: return ""Ключевое слово new"";
                case 3: return ""Идентификатор"";
                case 4: return ""Оператор конструктора"";
                case 5: return ""Оператор конструктора"";
                case 6: return ""Оператор перечисления"";
                case 7: return ""Оператор заверешения"";
                case 8: return ""Знак плюса"";
                case 9: return ""Знак минуса"";
                case 10: return ""Точка"";
                case 11: return """";
                case 12: return ""Цифра"";
                case 13: return ""Идентификатор"";
                case 14: return ""Разделитель"";
                case 15: return ""Переход на следующую строку"";
                case 16: return ""Недопустимый символ"";
                case -1: return ""Конец строки"";
                default: return ""Недопустимый символ"";
            }
        }

        // Меню - Справка

        private void Help_Click(object sender, EventArgs e)
        {
            Form help = new Form();
            help.Size = new System.Drawing.Size(500, 500);
            help.Text = ""Справочная служба"";
            help.StartPosition = FormStartPosition.CenterScreen;
            RichTextBox info = new RichTextBox();
            info.ReadOnly = true;
            info.Font = statusFont.Font;
            info.Size = new System.Drawing.Size(450, 425);
            info.Location = new Point(25, 15);
            info.Text = ""Меню приложения:\nФайл:\n - Создать файл - Создается новый файл и открывается в приложение для работы с ним. (Ctrl+N)\n"" +
                "" - Сохранить - Сохраняет текущий рабочий файл в приложении без указания пути до файла. (Ctrl+S)\n"" +
                "" - Cохранить как - Открывается диалоговое окно для указания расположения, где сохраниться текущий рабочий файл. (Ctrl+Shift+S)\n"" +
                "" - Выход - Совершается закрытие приложения. (Alt+F4)\n\nПравка:\n"" +
                "" - Отмена - Отменяет совершенное действие. (Ctrl+Z)\n"" +
                "" - Возврат - Возращает раннее отмененное действие. (Ctrl+Y)\n"" +
                "" - Вырезать - Стирает выделенный текст и сохраняет в буфере обмене для вставки. (Ctrl+X)\n"" +
                "" - Копировать - выделенный текст сохраняет в буфере обмене. (Ctrl+C)\n"" +
                "" - Вставить - Текст из буфере обмене напишеться в окне редактирования. (Ctrl+V)\n"" +
                "" - Удалить - Удаляет весь текст из окна редактирования. (Delete)\n"" +
                "" - Выделить все - Выделяет весь текст из окна редактирования. (Ctrl+A)\n\nПуск:\n"" +
                "" - Выводиться сообщение о том, что позже будет реализован пуск. (F5)\n\nСправка:\n"" +
                "" - Вызов справки - Появляется окно об всех функциях в приложении. (Ctrl+F1)\n"" +
                "" - О программе - Появляется окно с информации об окне. (Ctrl+F2)\n\nЛокализация:\n"" +
                "" - Русский - Меняет текст на русский язык. \n"" +
                "" - Китайский - Меняет текст на китайский язык. \n\nРазмер текста:\n"" +
                "" - прибавить - Увеличивает размер текста во всем приложении. (Ctrl+ +)\n"" +
                "" - убавить - Увеличивает размер текста во всем приложении. (Ctrl+ -)"";
            help.Controls.Add(info);
            help.ShowDialog();
        }
        private void AboutProgram_Click(object sender, EventArgs e)
        {
            Form about = new Form();
            about.Text = ""О программе"";
            about.StartPosition = FormStartPosition.CenterScreen;
            RichTextBox form = new RichTextBox();
            form.ReadOnly = true;
            form.Multiline = true;
            form.Dock = DockStyle.Fill;
            form.Text = ""Программа - пользовательский графический интерфейс для компилятора!\n"" +
                ""Выполнил: Тарбаев Даба-Цырен АП-327"" +
                ""\nПроверил: Антонянц Егор Николаевич асс. каф. АСУ."";
            about.Controls.Add(form);
            about.Show();
        }

        // Меню - Локализация

        private void RusLg_Click(object sender, EventArgs e)
        {
            swap_lang(0);
        }

        private void ChinaLg(object sender, EventArgs e)
        {
            swap_lang(1);
        }
        private void swap_lang(int key)
        {
            if (key == 0) файлToolStripMenuItem.Text = ""Файл"";
            else файлToolStripMenuItem.Text = ""文件"";

            if (key == 0) правкаToolStripMenuItem.Text = ""Правка"";
            else правкаToolStripMenuItem.Text = ""编辑"";

            if (key == 0) текстToolStripMenuItem.Text = ""Текст"";
            else текстToolStripMenuItem.Text = ""文本"";

            if (key == 0) пускToolStripMenuItem.Text = ""Пуск"";
            else пускToolStripMenuItem.Text = ""开始"";

            if (key == 0) справкаToolStripMenuItem.Text = ""Справка"";
            else справкаToolStripMenuItem.Text = ""参考"";

            if (key == 0) размерТекстаToolStripMenuItem.Text = ""Размер текста"";
            else размерТекстаToolStripMenuItem.Text = ""文字大小"";

            if (key == 0) локалиToolStripMenuItem.Text = ""Язык"";
            else локалиToolStripMenuItem.Text = ""本地化"";

            if (key == 0) созданиеToolStripMenuItem.Text = ""Создать"";
            else созданиеToolStripMenuItem.Text = ""创造"";

            if (key == 0) открытиеToolStripMenuItem.Text = ""Открыть"";
            else открытиеToolStripMenuItem.Text = ""打开"";

            if (key == 0) сохранениеToolStripMenuItem.Text = ""Сохранить"";
            else сохранениеToolStripMenuItem.Text = ""节省"";

            if (key == 0) сохранениеКакToolStripMenuItem.Text = ""Сохранить как"";
            else сохранениеКакToolStripMenuItem.Text = ""另存为"";

            if (key == 0) выходToolStripMenuItem.Text = ""Выход"";
            else выходToolStripMenuItem.Text = ""出口"";

            if (key == 0) MenuItemCancel.Text = ""Отменить"";
            else MenuItemCancel.Text = ""取消"";

            if (key == 0) MenuItemReturn.Text = ""Повторить"";
            else MenuItemReturn.Text = ""重复"";

            if (key == 0) MenuItemCut.Text = ""Вырезать"";
            else MenuItemCut.Text = ""切"";

            if (key == 0) MenuItemCopy.Text = ""Копировать"";
            else MenuItemCopy.Text = ""复制"";

            if (key == 0) MenuItemPaste.Text = ""Вставить"";
            else MenuItemPaste.Text = ""插入"";

            if (key == 0) MenuItemDelete.Text = ""Удалить"";
            else MenuItemDelete.Text = ""删除"";

            if (key == 0) русскийToolStripMenuItem.Text = ""Русский"";
            else русскийToolStripMenuItem.Text = ""俄语"";

            if (key == 0) китайскийToolStripMenuItem.Text = ""Китайский"";
            else китайскийToolStripMenuItem.Text = ""中文"";

            if (key == 0) увеличитьToolStripMenuItem.Text = ""Увеличить"";
            else увеличитьToolStripMenuItem.Text = ""增加"";

            if (key == 0) уменьшитьToolStripMenuItem.Text = ""Уменьшить"";
            else уменьшитьToolStripMenuItem.Text = ""减少"";

            if (key == 0) выделениеВсегоТекстаToolStripMenuItem.Text = ""Выделить все"";
            else выделениеВсегоТекстаToolStripMenuItem.Text = ""选择全部"";

            if (key == 0) постановкаЗадачиToolStripMenuItem.Text = ""Постановка задачи"";
            else постановкаЗадачиToolStripMenuItem.Text = ""问题陈述"";

            if (key == 0) грамматикаЯзыкаToolStripMenuItem.Text = ""Грамматика"";
            else грамматикаЯзыкаToolStripMenuItem.Text = ""语法"";

            if (key == 0) классификацияГрамматикиToolStripMenuItem.Text = ""Классификация грамматики"";
            else классификацияГрамматикиToolStripMenuItem.Text = ""语法分类"";

            if (key == 0) методАнализаToolStripMenuItem.Text = ""Метод анализа"";
            else методАнализаToolStripMenuItem.Text = ""分析方法"";

            if (key == 0) тестовыйПримерToolStripMenuItem.Text = ""Тестовый пример"";
            else тестовыйПримерToolStripMenuItem.Text = ""测试用例"";

            if (key == 0) списокЛитературыToolStripMenuItem.Text = ""Список литературы"";
            else списокЛитературыToolStripMenuItem.Text = ""参考"";

            if (key == 0) исходныйКодПрограммыToolStripMenuItem.Text = ""Исходный код программы"";
            else исходныйКодПрограммыToolStripMenuItem.Text = ""程序源码"";

            if (key == 0) вызовСправкиToolStripMenuItem.Text = ""Вызов справки"";
            else вызовСправкиToolStripMenuItem.Text = ""寻求帮助"";

            if (key == 0) оПрограммеToolStripMenuItem.Text = ""О программе"";
            else оПрограммеToolStripMenuItem.Text = ""关于该计划"";

            if (key == 0) outputBox.Columns[0].HeaderText = ""Путь файла"";
            else outputBox.Columns[0].HeaderText = ""文件路径"";

            if (key == 0) outputBox.Columns[1].HeaderText = ""Строка"";
            else outputBox.Columns[1].HeaderText = ""线"";

            if (key == 0) outputBox.Columns[2].HeaderText = ""Колонка"";
            else outputBox.Columns[2].HeaderText = ""柱子"";

            if (key == 0) outputBox.Columns[3].HeaderText = ""Сообщение"";
            else outputBox.Columns[3].HeaderText = ""信息"";

            if (key == 0) this.Text = ""Языковой процессор"";
            else this.Text = ""语言处理器"";

            if (key == 0) sec = ""сек"";
            else sec = ""第二"";

            if (key == 0) time = ""Время работы приложения: "";
            else time = ""申请开放时间"";

            int hours = totalSec / 3600;
            int minutes = (totalSec % 3600) / 60;
            int seconds = totalSec % 60;

            string timeStatus;

            if (hours > 0) timeStatus = $""{hours}:{minutes}:{seconds}"";
            else if (minutes > 0) timeStatus = $""{minutes}:{seconds}"";
            else timeStatus = $""{seconds} "" + sec;

            statusTimeApp.Text = time + timeStatus;
        }


        // Меню - Размер текста

        private void fontSize_Up(object sender, EventArgs e)
        {
            float size = buttonCancel.Font.Size;
            string font = buttonCancel.Font.Name;

            Size begin_size = this.Size;

            font_Change(this, size + 1, font);

            this.Size = begin_size;

            statusFont.Text = $""Шрифт: {font} {size + 1}pt"";
        }
        private void fontSize_Down(object sender, EventArgs e)
        {
            float size = buttonCancel.Font.Size;
            string font = buttonCancel.Font.Name;

            font_Change(this, size - 1, font);

            statusFont.Text = $""Шрифт: {font} {size - 1}pt"";
        }
        private void font_Change(Control control, float size, string font)
        {
            control.Font = new Font(font, size);
            foreach (Control sub in control.Controls)
            {
                font_Change(sub, size, font);
            }
        }

        // Строка состояния приложения
        private int totalSec = 0;
        private string sec = ""сек"";
        private string time = ""Время работы приложения: "";
        private void timerApp_Tick(object sender, EventArgs e)
        {
            totalSec++;

            int hours = totalSec / 3600;
            int minutes = (totalSec % 3600) / 60;
            int seconds = totalSec % 60;

            string timeStatus;

            if (hours > 0) timeStatus = $""{hours}:{minutes}:{seconds}"";
            else if (minutes > 0) timeStatus = $""{minutes}:{seconds}"";
            else timeStatus = $""{seconds} "" + sec;

            statusTimeApp.Text = time + timeStatus;
        }

        // Нумерация строк для окна редактирования

        private void UpdateLineNumbers(object sender)
        {
            RichTextBox inputBox = sender as RichTextBox;
            TabPage file = inputBox.Parent as TabPage;
            RichTextBox LineNumber = file.GetChildAtPoint(new Point(3, 3)) as RichTextBox;

            Point pt = new Point(0, 0);
            int First_Index = inputBox.GetCharIndexFromPosition(pt);
            int First_Line = inputBox.GetLineFromCharIndex(First_Index);

            pt.X = ClientRectangle.Width;
            pt.Y = ClientRectangle.Height;

            int Last_Index = inputBox.GetCharIndexFromPosition(pt);
            int Last_Line = inputBox.GetLineFromCharIndex(Last_Index);

            LineNumber.SelectionAlignment = HorizontalAlignment.Center;
            LineNumber.Text = """";
            for (int i = First_Line; i <= Last_Line; i++)
            {
                LineNumber.Text += i + 1 + ""\n"";
            }
        }
        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            if (curr_row != inputBox.GetLineFromCharIndex(inputBox.GetFirstCharIndexOfCurrentLine()) || curr_row == 0)
            {
                UpdateLineNumbers(sender);
                curr_row = inputBox.GetLineFromCharIndex(inputBox.GetFirstCharIndexOfCurrentLine());
            }
        }

        private void inputBox_VScroll(object sender, EventArgs e)
        {
            RichTextBox inputBox = sender as RichTextBox;
            TabPage file = inputBox.Parent as TabPage;
            RichTextBox LineNumber = file.GetChildAtPoint(new Point(3, 3)) as RichTextBox;

            LineNumber.Text = """";
            UpdateLineNumbers(sender);
            LineNumber.Invalidate();
        }
        private void inputBox_SelectionChanged(object sender, EventArgs e)
        {
            RichTextBox inputBox = sender as RichTextBox;
            Point pt = inputBox.GetPositionFromCharIndex(inputBox.SelectionStart);
            if (pt.X == 0)
            {
                UpdateLineNumbers(sender);
            }
        }

        private void LineNumber_MouseDown(object sender, MouseEventArgs e)
        {
            RichTextBox lineNumber = sender as RichTextBox;
            TabPage file = lineNumber.Parent as TabPage;
            RichTextBox inputBox = file.GetChildAtPoint(new Point(51, 3)) as RichTextBox;

            int charIndex = lineNumber.GetCharIndexFromPosition(e.Location);
            int visualLineIndex = lineNumber.GetLineFromCharIndex(charIndex);

            // Проверяем, есть ли текст в этой строке lineNumber
            if (visualLineIndex < lineNumber.Lines.Length)
            {
                string lineText = lineNumber.Lines[visualLineIndex].Trim();

                if (int.TryParse(lineText, out int lineNumberValue) && lineNumberValue > 0)
                {
                    int targetLine = lineNumberValue - 1;

                    if (targetLine >= 0 && targetLine < inputBox.Lines.Length)
                    {
                        int startIndex = inputBox.GetFirstCharIndexFromLine(targetLine);
                        int lineLength = inputBox.Lines[targetLine].Length;

                        inputBox.Select(startIndex, lineLength);

                        inputBox.Focus();
                    }
                }
            }
        }
        // Текущая строка
        private int curr_row = 0;
        // Текущее слово
        private string word = """";
        private void backfillkeywords(object sender, int pos, int len)
        {
            RichTextBox text_box = sender as RichTextBox;
            text_box.SelectionStart = pos;
            text_box.SelectionLength = len;
            text_box.SelectionBackColor = Color.Yellow;
            text_box.SelectionStart = pos + len;
            text_box.SelectionLength = 0;
            text_box.SelectionBackColor = Color.White;
        }
        // Подсветка ключевых слов
        private void inputBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            RichTextBox text_box = sender as RichTextBox;
            if (e.KeyChar == '\r' || e.KeyChar == ' ' || e.KeyChar == '\0' || e.KeyChar == (char)Keys.Back)
            {
                int curr_pos = 0;
                if (e.KeyChar == '\r') curr_row--;
                switch (word.ToLower())
                {
                    case ""if"":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case ""else"":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case ""int"":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case ""float"":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case ""while"":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                    case ""for"":
                        curr_pos = text_box.GetFirstCharIndexFromLine(curr_row) + text_box.Lines[curr_row].IndexOf(word);
                        backfillkeywords(text_box, curr_pos, word.Length);
                        e.Handled = true;
                        break;
                }
                word = """";
            }
            else word += e.KeyChar;
        }

        private void FormGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result;
            if (files.SelectedIndex > -1)
            {
                result = MessageBox.Show(""У вас есть несохраненный файл, сохранить?"", ""сообщение"", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    сохранениеКакToolStripMenuItem.PerformClick();
                }
            }
        }
        public class ReportFile
        {
            public List<string> path = new List<string>();
            public List<string> line = new List<string>();
            public List<string> column = new List<string>();
            public List<string> message = new List<string>();
            public ReportFile(string path = """", string line = ""0"", string column = ""0"", string message = """")
            {
                addReport(path, line, column, message);
            }
            public void addReport(string path = """", string line = ""0"", string column = ""0"", string message = """")
            {
                this.path.Add(path);
                this.line.Add(line);
                this.column.Add(column);
                this.message.Add(message);
            }
        }

        private void outputBox_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            inputBox.Focus();
            if (e.RowIndex == -1) return;
            string position = outputBox.Rows[e.RowIndex].Cells[3].Value.ToString();
            string location = position.Split(' ')[1];
            string line = location.Split(',')[0];
            int numberLine = Convert.ToInt32(line);
            string inLine = position.Split(' ')[2];
            int positioninstr = Convert.ToInt32(inLine.Split('-')[0]);
            inputBox.Select(inputBox.GetFirstCharIndexFromLine(numberLine - 1) + positioninstr - 1, 1);
        }

        private void outputBoxANTLR_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            inputBox.Focus();
            if (e.RowIndex == -1) return;
            string position = outputBoxANTLR.Rows[e.RowIndex].Cells[3].Value.ToString();
            string location = position.Split(' ')[1];
            string line = location.Split(',')[0];
            int numberLine = Convert.ToInt32(line);
            string inLine = position.Split(' ')[2];
            int positioninstr = Convert.ToInt32(inLine.Split('-')[0]);
            inputBox.Select(inputBox.GetFirstCharIndexFromLine(numberLine - 1) + positioninstr - 1, 1);
        }

        private void постановкаЗадачиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = ""Постановка задачи"";
            form.Size = new Size(700, 400);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font(""Times New Roman"", 14);
            textBox.Text = @""   Постановка задачи

    Комплексное число — это число, которое состоит из реальной части числа и мнимой части числа. Комплексное число z обычно записывается в форме z = x + yi, где x и y являются реальными числами, и i — мнимая единица, которая имеет свойство i² = -1.

    Для объявления комплексного числа с инициализацией на языке C# используется ключевое слово Complex.

    Формат записи: """"Complex имя_переменной = new Complex(число1, число2);""""

    Примеры верных записей:
    1) """"Complex c1 = new Complex(1.2, 6.0);""""
    2) """"Complex comp1 = new Complex(146, -6);""""
    3) """"Complex number = new Complex(-1.252, 180);"""""";

            form.Controls.Add(textBox);
            form.Show();
        }

        private void грамматикаЯзыкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = ""Разработанная грамматика"";
            form.Size = new Size(800, 600);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font(""Times New Roman"", 14);
            textBox.Text = @""   Разработанная грамматика
    
    Определим грамматику объявления комплексного числа с инициализацией на языке C# G[<START>] в нотации Хомского с продукциями P:

    G[<START>]:
    1)  <START> → 'Complex'<ID>
    2)  <ID> → letter<IDREM>
    3)  <IDREM> → letter<IDREM> | digit<IDREM> | '='<NEW>
    4)  <NEW> → 'new'<TYPE>
    5)  <TYPE> → 'Complex'<OPEN_PAREN>
    6)  <OPEN_PAREN> → '('<SIGN>
    7)  <SIGN> → '+'<DIGIT_REAL> | '-'<DIGIT_REAL> | digit<REAL>
    8)  <DIGIT_REAL> → digit<REAL>
    9)  <REAL> → digit<REAL> | '.'<REAL_DOT> | ','<IMAG_SIGN>
    10) <REAL_DOT> → digit<REAL_FRACTION>
    11) <REAL_FRACTION> → digit<REAL_FRACTION> | ','<IMAG_SIGN>
    12) <IMAG_SIGN> → '+'<DIGIT_IMAG> | '-'<DIGIT_IMAG> | digit<IMAG>
    13) <DIGIT_IMAG> → digit<IMAG>
    14) <IMAG> → digit<IMAG> | '.'<IMAG_DOT> | ')'<END>
    15) <IMAG_DOT> → digit<IMAG_FRACTION>
    16) <IMAG_FRACTION> → digit<IMAG_FRACTION> | ')'<END>
    17) <END> → ';'

    letter → 'a' | 'b' | ... | 'z' | 'A' | 'B' | ... | 'Z'
    digit → '0' | '1' | ... | '9'

    Следуя введенному формальному определению грамматики, представим G[<START>] ее составляющими:

    Z = <START>

    VT = {a, b, ..., z, A, B, ..., Z, 0, 1, ..., 9, +, -, ., ,, (, ), ;}

    VN = {<START>, <ID>, <IDREM>, <NEW>, <TYPE>, <OPEN_PAREN>, <SIGN>, 
        <DIGIT_REAL>, <REAL>, <REAL_DOT>, <REAL_FRACTION>, <IMAG_SIGN>, 
        <DIGIT_IMAG>, <IMAG>, <IMAG_DOT>, <IMAG_FRACTION>, <END>}"";

            form.Controls.Add(textBox);
            form.Show();
        }

        private void классификацияГрамматикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = ""Классификация грамматики"";
            form.Size = new Size(800, 600);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font(""Times New Roman"", 14);
            textBox.Text = @""   Классификация грамматики

    Согласно классификации Хомского, грамматика G[<START>] является автоматной.

    Правила (1)-(17) относятся к классу праворекурсивных продукций (A → aB | a | ε):

    1)  <START> → 'Complex'<ID>
    2)  <ID> → letter<IDREM>
    3)  <IDREM> → letter<IDREM> | digit<IDREM> | '='<NEW>
    4)  <NEW> → 'new'<TYPE>
    5)  <TYPE> → 'Complex'<OPEN_PAREN>
    6)  <OPEN_PAREN> → '('<SIGN>
    7)  <SIGN> → '+'<DIGIT_REAL> | '-'<DIGIT_REAL> | digit<REAL>
    8)  <DIGIT_REAL> → digit<REAL>
    9)  <REAL> → digit<REAL> | '.'<REAL_DOT> | ','<IMAG_SIGN>
    10) <REAL_DOT> → digit<REAL_FRACTION>
    11) <REAL_FRACTION> → digit<REAL_FRACTION> | ','<IMAG_SIGN>
    12) <IMAG_SIGN> → '+'<DIGIT_IMAG> | '-'<DIGIT_IMAG> | digit<IMAG>
    13) <DIGIT_IMAG> → digit<IMAG>
    14) <IMAG> → digit<IMAG> | '.'<IMAG_DOT> | ')'<END>
    15) <IMAG_DOT> → digit<IMAG_FRACTION>
    16) <IMAG_FRACTION> → digit<IMAG_FRACTION> | ')'<END>
    17) <END> → ';'

    Отметим, что правила должны быть либо только леворекурсивными, либо только праворекурсивными. Комбинация тех и других не допускается. Данная грамматика не содержит леворекурсивных продукций, все рекурсивные правила являются праворекурсивными, следовательно, грамматика является автоматной (праволинейной)."";

            form.Controls.Add(textBox);
            form.Show();
        }

        private void методАнализаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = ""Метод анализа"";
            form.Size = new Size(800, 600);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            
            textBox.Font = new Font(""Times New Roman"", 14);
            // Текст с выравниванием по центру
           
            textBox.SelectionFont = new Font(""Times New Roman"", 14, FontStyle.Regular);
            textBox.SelectionAlignment = HorizontalAlignment.Left;
            textBox.AppendText(""Метод анализа\n\n"");
            textBox.AppendText(""На рисунке 1 представлена посимвольная декомпозиция объявления комплексного числа "" +
                              ""с генерацией соответствующего символического кода символов <ID> и т.д., и литеры «+», «–» и др. "" +
                              ""Непомеченные дуги на диаграмме соответствуют состоянию ERROR (отсутствие данного символа в "" +
                              ""словаре грамматики) либо выходу из обработки очередного символа и переходу на старт обработки следующего.\n\n"");

            textBox.SelectionAlignment = HorizontalAlignment.Center;
            
            Clipboard.SetImage(Properties.Resources.diagram_states);
            textBox.Paste();
            textBox.AppendText(""\nРисунок 1 – Диаграмма состояний\n"");
            textBox.AppendText(""\n\n"");

            textBox.SelectionAlignment = HorizontalAlignment.Left;
            textBox.AppendText(""Грамматика G[<START>] является автоматной.\n"");
            textBox.AppendText(""Правила (1) – (17) для G[<START>] реализованы на графе (см. рисунок 2).\n"");
            textBox.AppendText(""Сплошные стрелки на графе характеризуют синтаксически верный разбор. "" +
                              ""Состояние 18 символизирует успешное завершение разбора.\n\n"");

            textBox.SelectionAlignment = HorizontalAlignment.Center;
            
            Clipboard.SetImage(Properties.Resources.graph_grammar);
            textBox.Paste();
            textBox.AppendText(""\nРисунок 2 – Граф G[<START>]\n"");

            textBox.ReadOnly = true;
            textBox.SelectionStart = 0;
            textBox.SelectionLength = 0;

            form.Controls.Add(textBox);
            form.Show();
        }

        private void тестовыйПримерToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = ""Метод анализа"";
            form.Size = new Size(800, 600);
            form.StartPosition = FormStartPosition.CenterScreen;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;

            textBox.Font = new Font(""Times New Roman"", 14);
            // Текст с выравниванием по центру

            textBox.SelectionFont = new Font(""Times New Roman"", 14, FontStyle.Regular);
            textBox.SelectionAlignment = HorizontalAlignment.Left;
            textBox.AppendText(""Тестовые примеры\n\n"");
            textBox.AppendText(""На рисунках представлены тестовые примеры запуска разработанного лексического анализатора для объявления комплексного числа с инициализацией на языке C#.\n"");
            textBox.AppendText(""1) “Complex c1 = new Complex(1.2, 6.0);”\n"");
            Clipboard.SetImage(Properties.Resources.test1);
            textBox.Paste();
            textBox.AppendText(""\n2) “Complex comp1 = new Complex(146, -6);”\n"");
            Clipboard.SetImage(Properties.Resources.test2);
            textBox.Paste();
            textBox.AppendText(""\n3) “Complexname = new Com@plex(10, -6.76);”\n"");
            Clipboard.SetImage(Properties.Resources.test3);
            textBox.Paste();
            textBox.AppendText(""\n4) “Complex idddd === # new Complex((32,32);”\n"");
            Clipboard.SetImage(Properties.Resources.test4);
            textBox.Paste();
            textBox.AppendText(""\n5) “Complexxx +21name=new Complex(#,3);”\n"");
            Clipboard.SetImage(Properties.Resources.test5);
            textBox.Paste();

            textBox.ReadOnly = true;
            textBox.SelectionStart = 0;
            textBox.SelectionLength = 0;

            form.Controls.Add(textBox);
            form.Show();
        }

        private void списокЛитературыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = ""Список использованных источников"";
            form.Size = new Size(600, 300);
            form.StartPosition = FormStartPosition.CenterParent;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font(""Times New Roman"", 14, FontStyle.Regular);
            textBox.Text = @""Список использованных источников

1. Шорников Ю. В. Теория языков программирования: проектирование и реализация : учебное пособие / Ю. В. Шорников. — Новосибирск : Изд-во НГТУ, 2022. — 290 с.

2. Gries D. Designing Compilers for Digital Computers. New York, Jhon Wiley, 1971. 493 p.

3. Теория формальных языков и компиляторов [Электронный ресурс] / Электрон. дан. URL: https://dispace.edu.nstu.ru/didesk/course/show/8594, свободный. Яз.рус. (дата обращения 14.04.2026)."";

            form.Controls.Add(textBox);
            form.Show();
        }

        private void исходныйКодПрограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = ""Исходный код программы"";
            form.Size = new Size(700, 500);
            form.StartPosition = FormStartPosition.CenterParent;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.Font = new Font(""Consolas"", 9);
            textBox.WordWrap = false;
            textBox.ScrollBars = RichTextBoxScrollBars.Both;
            textBox.Text = @""Исходный код программы (scanner.cs)
using System;
using System.Collections.Generic;

namespace Scanner
{
    public class Token
    {
        public readonly int id;
        public readonly string type;
        public readonly string name;
        public readonly string location;
        public Token(int id, string type, string name, string location)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.location = location;
        }
    }
    public class scanner
    {
        private string text;
        private char liter;
        private int currentPosition = 0;
        private int positionLine = 0;
        private int currentLine = 1;
        private string buffer = """""""";
        private List<Token> tokens = new List<Token>();
        public List<Token> analyze(string inputText)
        {
            text = inputText;

            getNext();

            while (currentPosition <= text.Length)
            {
                if (char.IsLetter(liter))
                {
                    buffer += liter;
                    while (char.IsLetterOrDigit(liter = getChar()))
                    {
                        buffer += liter;
                    }
                    switch (buffer)
                    {
                        case """"Complex"""":
                            addToken(1, """"Ключевое слово Complex"""", buffer);
                            break;
                        case """"new"""":
                            addToken(2, """"Ключевое слово new"""", buffer);
                            break;
                        default:
                            addToken(3, """"Идентификатор"""", buffer);
                            break;
                    }
                    buffer = """""""";
                }
                else if (char.IsDigit(liter))
                {
                    buffer += liter;
                    while (char.IsDigit(liter = getChar()))
                    {
                        buffer += liter;
                    }
                    if (liter == '.')
                    {
                        buffer += liter;
                        while (char.IsDigit(liter = getChar()))
                        {
                            buffer += liter;
                        }
                        addToken(11, """"Вещественное число"""", buffer);
                    }
                    else
                    {
                        addToken(10, """"Целое без знака"""", buffer);
                    }
                    buffer = """""""";
                }
                else
                {
                    switch (liter)
                    {
                        case '\0':
                            getNext();
                            break;
                        case '\n':
                            positionLine = 0;
                            currentLine++;
                            getNext();
                            break;
                        case '=':
                            buffer += liter;
                            while ((liter = getChar()) == '=')
                            {
                                buffer += liter;
                            }
                            addToken(4, """"Оператор присваивания"""", buffer);
                            buffer = """""""";
                            break;
                        case ' ':
                            buffer += liter;
                            while ((liter = getChar()) == ' ')
                            {
                                buffer += liter;
                            }
                            addToken(5, """"Разделитель"""", buffer);
                            buffer = """""""";
                            break;
                        case '(':
                            buffer += liter;
                            while ((liter = getChar()) == '(')
                            {
                                buffer += liter;
                            }
                            addToken(6, """"Оператор конструктора"""", buffer);
                            buffer = """""""";
                            break;
                        case ')':
                            buffer += liter;
                            while ((liter = getChar()) == ')')
                            {
                                buffer += liter;
                            }
                            addToken(7, """"Оператор конструктора"""", buffer);
                            buffer = """""""";
                            break;
                        case '-':
                            buffer += liter;
                            while ((liter = getChar()) == '-')
                            {
                                buffer += liter;
                            }
                            addToken(8, """"Знак минуса"""", buffer);
                            buffer = """""""";
                            break;
                        case '+':
                            buffer += liter;
                            while ((liter = getChar()) == '+')
                            {
                                buffer += liter;
                            }
                            addToken(9, """"Знак плюса"""", buffer);
                            buffer = """""""";
                            break;
                        case ',':
                            buffer += liter;
                            while ((liter = getChar()) == ',')
                            {
                                buffer += liter;
                            }
                            addToken(12, """"Оператор перечисления"""", buffer);
                            buffer = """""""";
                            break;
                        case ';':
                            buffer += liter;
                            while ((liter = getChar()) == ';')
                            {
                                buffer += liter;
                            }
                            addToken(13, """"Оператор заверешения"""", buffer);
                            buffer = """""""";
                            break;
                        default:
                            buffer += liter;
                            while (!(char.IsLetterOrDigit(liter = getChar()) || liter == '\0' || liter == '\n' || liter == ' '
                                || liter == '(' || liter == ')' || liter == '=' || liter == '-' || liter == '+' || liter == ',' || liter == ';'))
                            {
                                buffer += liter;
                            }
                            addToken(-1, """"Недопустимый символ"""", buffer);
                            buffer = """""""";
                            break;
                    }
                }
            }

            return tokens;
        }
        private char getChar()
        {
            try
            {
                if (currentPosition >= text.Length)
                {
                    currentPosition++;
                    positionLine++;
                    return '\0';
                }
                char liter1 = text[currentPosition];
                currentPosition++;
                positionLine++;
                return liter1;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception(""""В конце строки не обнаружено ;"""");
            }
        }
        private void getNext()
        {
            liter = getChar();
        }
        private void addToken(int id, string type, string name)
        {
            int Length = name.Length;
            int leng = positionLine - Length;
            string loc = $""""строка {currentLine}, {leng}-{positionLine - 1}"""";
            tokens.Add(new Token(id, type, name, loc));
        }
    }
}

"";
            form.Controls.Add(textBox);
            form.Show();
        }
    }
    class ErrorListener : BaseErrorListener
    {
        public List<String> stringArray = new List<string>();
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            stringArray.Add(msg + "" Line: "" + line + "" Char position in line: "" + charPositionInLine);
        }
    }
    class ErrorLexerListener : IAntlrErrorListener<int>
    {
        public List<String> stringArray = new List<string>();
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            stringArray.Add(msg + "" Line: "" + line + "" Char position in line: "" + charPositionInLine);
        }
    }
}


";
            form.Controls.Add(textBox);
            form.Show();
        }

    }
}
