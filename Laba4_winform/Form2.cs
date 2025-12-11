using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Laba4_winform
{
    public partial class Form2 : Form
    {
        private OpenFileDialog openFileDialog1;
        private SaveFileDialog saveFileDialog1;
        private DataGridViewTextBoxEditingControl currentEditingControl;
        private bool isCalculatingBMI = false;

        public Form2()
        {
            InitializeComponent();
            InitializeOpenFileDialog();
            InitializeSaveFileDialog();
            this.Shown += Form2_Shown;
        }

        private void InitializeOpenFileDialog()
        {
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Выберите текстовый файл с данными";
        }

        private void InitializeSaveFileDialog()
        {
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.Title = "Сохранить файл данных";
            saveFileDialog1.DefaultExt = "txt";
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            OpenAndLoadFile();
        }

        private void OpenAndLoadFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = openFileDialog1.FileName;
                    LoadFile(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void LoadFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не найден!");
                    this.Close();
                    return;
                }

                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст!");
                    this.Close();
                    return;
                }

                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();

                var headers = lines[0].Split('|');

                foreach (var header in headers)
                {
                    dataGridView1.Columns.Add(header.Trim(), header.Trim());
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    var data = lines[i].Split('|');

                    // Если в файле уже есть ИМТ, используем его, иначе рассчитываем
                    if (data.Length == headers.Length)
                    {
                        dataGridView1.Rows.Add(data);
                    }
                    else if (data.Length == 4) // Если нет столбца ИМТ
                    {
                        // Добавляем пустое значение для ИМТ
                        var rowData = data.ToList();
                        rowData.Add("");
                        dataGridView1.Rows.Add(rowData.ToArray());
                    }
                    else
                    {
                        dataGridView1.Rows.Add(data);
                    }
                }

                this.Text = "Данные из файла: " + Path.GetFileName(filePath);

                // Настройка DataGridView для редактирования
                ConfigureDataGridView();

                // Автоматически рассчитываем ИМТ для всех строк
                CalculateAllBMI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                this.Close();
            }
        }

        private void ConfigureDataGridView()
        {
            // Разрешаем редактирование
            dataGridView1.ReadOnly = false;
            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            // Настраиваем обработчики событий для валидации ввода
            dataGridView1.EditingControlShowing += DataGridView1_EditingControlShowing;
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
            dataGridView1.DataError += DataGridView1_DataError;
            dataGridView1.RowsAdded += DataGridView1_RowsAdded;
        }

        private void DataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Убираем предыдущий обработчик
            if (currentEditingControl != null)
            {
                currentEditingControl.KeyPress -= EditingControl_KeyPress;
            }

            currentEditingControl = e.Control as DataGridViewTextBoxEditingControl;
            if (currentEditingControl != null)
            {
                currentEditingControl.KeyPress += EditingControl_KeyPress;
            }
        }

        private void EditingControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            int columnIndex = dataGridView1.CurrentCell.ColumnIndex;
            string columnName = dataGridView1.Columns[columnIndex].HeaderText.ToLower();

            // Разрешаем управляющие клавиши
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            switch (columnName)
            {
                case "фио":
                case "имя фамилия":
                case "имя":
                case "фамилия":
                    // Разрешаем только буквы, пробел, дефис и апостроф
                    if (!char.IsLetter(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != '\'')
                    {
                        e.Handled = true;
                    }
                    break;

                case "возраст":
                case "рост":
                    // Разрешаем только цифры для возраста и роста
                    if (!char.IsDigit(e.KeyChar))
                    {
                        e.Handled = true;
                    }
                    break;

                case "вес":
                    // Разрешаем цифры и точку для веса
                    if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    {
                        e.Handled = true;
                    }

                    // Проверяем, что точка только одна
                    TextBox textBox = sender as TextBox;
                    if (textBox != null && e.KeyChar == '.' && textBox.Text.Contains('.'))
                    {
                        e.Handled = true;
                    }
                    break;

                case "имт":
                    // Столбец ИМТ должен быть только для чтения
                    e.Handled = true;
                    break;
            }
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Если изменился вес или рост, пересчитываем ИМТ
            if (e.RowIndex >= 0 && !isCalculatingBMI)
            {
                string columnName = dataGridView1.Columns[e.ColumnIndex].HeaderText.ToLower();

                if (columnName == "вес" || columnName == "рост")
                {
                    CalculateBMIForRow(e.RowIndex);
                }
            }
        }

        private void CalculateBMIForRow(int rowIndex)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count)
                    return;

                DataGridViewRow row = dataGridView1.Rows[rowIndex];

                // Если это новая пустая строка, выходим
                if (row.IsNewRow)
                    return;

                // Получаем значения веса и роста
                string weightStr = row.Cells["Вес"]?.Value?.ToString();
                string heightStr = row.Cells["Рост"]?.Value?.ToString();

                // Ищем столбец с названием "Вес" (регистронезависимо)
                if (string.IsNullOrEmpty(weightStr))
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (dataGridView1.Columns[cell.ColumnIndex].HeaderText.ToLower() == "вес")
                        {
                            weightStr = cell.Value?.ToString();
                            break;
                        }
                    }
                }

                // Ищем столбец с названием "Рост" (регистронезависимо)
                if (string.IsNullOrEmpty(heightStr))
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (dataGridView1.Columns[cell.ColumnIndex].HeaderText.ToLower() == "рост")
                        {
                            heightStr = cell.Value?.ToString();
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(weightStr) || string.IsNullOrWhiteSpace(heightStr))
                    return;

                if (!double.TryParse(weightStr, out double weight) || !double.TryParse(heightStr, out double height))
                    return;

                // Проверяем диапазоны
                if (weight < 30 || weight > 300 || height < 120 || height > 230)
                    return;

                // Находим столбец ИМТ
                int bmiColumnIndex = -1;
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    if (dataGridView1.Columns[i].HeaderText.ToLower().Contains("имт"))
                    {
                        bmiColumnIndex = i;
                        break;
                    }
                }

                // Если столбца ИМТ нет, добавляем его
                if (bmiColumnIndex == -1)
                {
                    dataGridView1.Columns.Add("ИМТ", "ИМТ");
                    bmiColumnIndex = dataGridView1.Columns.Count - 1;
                }

                // Устанавливаем ИМТ для столбца (делаем его read-only)
                dataGridView1.Columns[bmiColumnIndex].ReadOnly = true;

                // Рассчитываем ИМТ: вес / (рост в метрах)^2
                double heightInMeters = height / 100.0;
                double bmi = weight / (heightInMeters * heightInMeters);

                // Сохраняем ИМТ с округлением до 2 знаков
                isCalculatingBMI = true;
                row.Cells[bmiColumnIndex].Value = Math.Round(bmi, 2).ToString();
                isCalculatingBMI = false;
            }
            catch (Exception ex)
            {
                isCalculatingBMI = false;
                Console.WriteLine($"Ошибка при расчете ИМТ: {ex.Message}");
            }
        }

        private void CalculateAllBMI()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (!dataGridView1.Rows[i].IsNewRow)
                {
                    CalculateBMIForRow(i);
                }
            }
        }

        private bool ValidateAllData()
        {
            bool isValid = true;
            List<string> errorMessages = new List<string>();

            for (int row = 0; row < dataGridView1.Rows.Count - 1; row++) // -1 чтобы исключить новую пустую строку
            {
                DataGridViewRow dataRow = dataGridView1.Rows[row];
                dataRow.ErrorText = ""; // Очищаем предыдущие ошибки

                // Проверяем каждую ячейку в строке
                for (int col = 0; col < dataGridView1.Columns.Count; col++)
                {
                    string columnName = dataGridView1.Columns[col].HeaderText.ToLower();
                    string value = dataRow.Cells[col]?.Value?.ToString() ?? "";

                    // Для столбца ИМТ пропускаем проверку на пустоту - он рассчитывается автоматически
                    if (columnName.Contains("имт"))
                        continue;

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        isValid = false;
                        errorMessages.Add($"Строка {row + 1}: Пустое значение в столбце '{dataGridView1.Columns[col].HeaderText}'");
                        continue;
                    }

                    bool cellValid = true;
                    string errorMsg = "";

                    switch (columnName)
                    {
                        case "фио":
                        case "имя фамилия":
                        case "имя":
                        case "фамилия":
                            cellValid = ValidateFIO(value);
                            errorMsg = "Некорректное ФИО (только буквы, пробелы, дефисы, макс 100 символов)";
                            break;

                        case "возраст":
                            cellValid = ValidateAge(value);
                            errorMsg = "Некорректный возраст (10-80 лет)";
                            break;

                        case "рост":
                            cellValid = ValidateHeight(value);
                            errorMsg = "Некорректный рост (120-230 см)";
                            break;

                        case "вес":
                            cellValid = ValidateWeight(value);
                            errorMsg = "Некорректный вес (30-300 кг)";
                            break;
                    }

                    if (!cellValid)
                    {
                        isValid = false;
                        errorMessages.Add($"Строка {row + 1}: {errorMsg}");

                        // Добавляем ошибку к строке
                        if (string.IsNullOrEmpty(dataRow.ErrorText))
                        {
                            dataRow.ErrorText = errorMsg;
                        }
                        else
                        {
                            dataRow.ErrorText += $"; {errorMsg}";
                        }
                    }
                }
            }

            if (!isValid)
            {
                string errorMessage = "Обнаружены ошибки в данных:\n" + string.Join("\n", errorMessages.Take(10));
                if (errorMessages.Count > 10)
                {
                    errorMessage += $"\n... и ещё {errorMessages.Count - 10} ошибок";
                }
                MessageBox.Show(errorMessage, "Ошибка валидации",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return isValid;
        }

        private bool ValidateFIO(string fio)
        {
            if (string.IsNullOrWhiteSpace(fio))
                return false;

            if (fio.Length > 100)
                return false;

            // Разрешаем только русские и английские буквы, пробелы, дефисы и апострофы
            Regex regex = new Regex(@"^[a-zA-Zа-яА-ЯёЁ\s\-\']+$");
            return regex.IsMatch(fio);
        }

        private bool ValidateAge(string age)
        {
            if (string.IsNullOrWhiteSpace(age))
                return false;

            if (!int.TryParse(age, out int ageValue))
                return false;

            return ageValue >= 10 && ageValue <= 80;
        }

        private bool ValidateHeight(string height)
        {
            if (string.IsNullOrWhiteSpace(height))
                return false;

            if (!int.TryParse(height, out int heightValue))
                return false;

            return heightValue >= 120 && heightValue <= 230;
        }

        private bool ValidateWeight(string weight)
        {
            if (string.IsNullOrWhiteSpace(weight))
                return false;

            // Разрешаем десятичные значения для веса
            if (!double.TryParse(weight, out double weightValue))
                return false;

            return weightValue >= 30 && weightValue <= 300;
        }

        // Кнопка "Сохранить"
        private void button1_Click(object sender, EventArgs e)
        {
            // Автоматически пересчитываем ИМТ перед сохранением
            CalculateAllBMI();

            // Проверяем все данные перед сохранением
            if (!ValidateAllData())
            {
                MessageBox.Show("Исправьте ошибки в данных перед сохранением",
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Открываем диалог сохранения файла
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SaveDataToFile(saveFileDialog1.FileName);
                    MessageBox.Show($"Данные успешно сохранены в файл:\n{saveFileDialog1.FileName}",
                        "Сохранение завершено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Кнопка "Удалить"
        private void button2_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли строка
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строку для удаления!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Получаем выбранную строку
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

            // Проверяем, что это не новая пустая строка
            if (selectedRow.IsNewRow)
            {
                MessageBox.Show("Невозможно удалить пустую строку!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Получаем ФИО из выбранной строки
            string fio = GetFIOFromRow(selectedRow);

            // Спрашиваем подтверждение у пользователя
            DialogResult result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пользователя:\n\n{fio}?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            // Если пользователь подтвердил удаление
            if (result == DialogResult.Yes)
            {
                try
                {
                    // Удаляем строку
                    dataGridView1.Rows.Remove(selectedRow);

                    MessageBox.Show($"Пользователь '{fio}' успешно удален!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении строки: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Кнопка "Поиск"
        private void button3_Click(object sender, EventArgs e)
        {
            SearchPerson();
        }

        // Кнопка "Максимальный рост"
        private void button6_Click(object sender, EventArgs e)
        {
            FindPersonWithMaxHeight();
        }

        // Метод для нахождения человека с максимальным ростом
        private void FindPersonWithMaxHeight()
        {
            // Проверяем, есть ли данные в таблице
            if (dataGridView1.Rows.Count <= 1) // только новая пустая строка
            {
                MessageBox.Show("В таблице нет данных для поиска!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Переменные для хранения информации о максимальном росте
            int maxHeight = 0;
            int rowIndexWithMaxHeight = -1;
            string fioWithMaxHeight = "";
            bool foundValidData = false;

            // Очищаем предыдущее выделение
            dataGridView1.ClearSelection();

            // Проходим по всем строкам DataGridView (кроме новой пустой строки)
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                DataGridViewRow row = dataGridView1.Rows[i];

                // Получаем значение роста из текущей строки
                string heightStr = GetHeightFromRow(row);

                if (!string.IsNullOrWhiteSpace(heightStr) && int.TryParse(heightStr, out int height))
                {
                    // Проверяем валидность роста
                    if (height >= 120 && height <= 230)
                    {
                        foundValidData = true;

                        // Сравниваем с текущим максимальным ростом
                        if (height > maxHeight)
                        {
                            maxHeight = height;
                            rowIndexWithMaxHeight = i;
                            fioWithMaxHeight = GetFIOFromRow(row);
                        }
                        else if (height == maxHeight && rowIndexWithMaxHeight == -1)
                        {
                            // Если рост одинаковый и еще не нашли максимум
                            rowIndexWithMaxHeight = i;
                            fioWithMaxHeight = GetFIOFromRow(row);
                        }
                    }
                }
            }

            // Проверяем, нашли ли мы данные
            if (!foundValidData)
            {
                MessageBox.Show("Не удалось найти корректные данные о росте в таблице!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (rowIndexWithMaxHeight == -1)
            {
                MessageBox.Show("Не удалось определить человека с максимальным ростом!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Выделяем строку с максимальным ростом
            DataGridViewRow maxHeightRow = dataGridView1.Rows[rowIndexWithMaxHeight];
            maxHeightRow.Selected = true;

            // Прокручиваем к найденной строке
            dataGridView1.FirstDisplayedScrollingRowIndex = rowIndexWithMaxHeight;

            // Показываем информацию о найденном человеке
            MessageBox.Show($"Найден человек с максимальным ростом:\n\n" +
                          $"ФИО: {fioWithMaxHeight}\n" +
                          $"Рост: {maxHeight} см",
                          "Максимальный рост",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);

            // Фокусируемся на DataGridView
            dataGridView1.Focus();
        }

        // Метод для получения роста из строки DataGridView
        private string GetHeightFromRow(DataGridViewRow row)
        {
            // Ищем столбец с ростом (регистронезависимо)
            foreach (DataGridViewCell cell in row.Cells)
            {
                string columnName = dataGridView1.Columns[cell.ColumnIndex].HeaderText.ToLower();
                if (columnName == "рост" || columnName.Contains("рост"))
                {
                    return cell.Value?.ToString()?.Trim() ?? "";
                }
            }

            return "";
        }

        // Метод для поиска человека
        private void SearchPerson()
        {
            // Получаем текст из TextBox для поиска
            string searchText = textBox1.Text.Trim();

            // Проверяем, не пустой ли запрос
            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Введите имя или фамилию для поиска!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Focus();
                return;
            }

            // Очищаем предыдущее выделение
            dataGridView1.ClearSelection();

            // Флаг для отслеживания, найден ли человек
            bool found = false;

            // Проходим по всем строкам DataGridView (кроме новой пустой строки)
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                DataGridViewRow row = dataGridView1.Rows[i];

                // Получаем ФИО из текущей строки
                string fio = GetFIOFromRow(row);

                // Ищем совпадение (регистронезависимо)
                if (fio.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Выделяем найденную строку
                    row.Selected = true;

                    // Прокручиваем к найденной строке
                    dataGridView1.FirstDisplayedScrollingRowIndex = i;

                    // Устанавливаем флаг, что нашли
                    found = true;

                    // Можно прервать поиск после первого найденного,
                    // или убрать break, чтобы найти все совпадения
                    // break;
                }
            }

            // Если не нашли ни одного совпадения
            if (!found)
            {
                MessageBox.Show($"Человек с именем или фамилией '{searchText}' не найден.",
                    "Результат поиска", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Если нашли - фокусируемся на DataGridView
                dataGridView1.Focus();
            }
        }

        // Метод для получения ФИО из строки DataGridView
        private string GetFIOFromRow(DataGridViewRow row)
        {
            string fio = "";

            // Ищем столбец с ФИО (регистронезависимо)
            foreach (DataGridViewCell cell in row.Cells)
            {
                string columnName = dataGridView1.Columns[cell.ColumnIndex].HeaderText.ToLower();
                if (columnName == "фио" || columnName == "имя фамилия" ||
                    columnName == "имя" || columnName.Contains("фио") ||
                    columnName.Contains("имя фамилия"))
                {
                    fio = cell.Value?.ToString() ?? "Неизвестный пользователь";
                    break;
                }
            }

            // Если не нашли ФИО в столбцах, берем значение из первого столбца
            if (string.IsNullOrEmpty(fio))
            {
                fio = row.Cells[0].Value?.ToString() ?? "Неизвестный пользователь";
            }

            return fio;
        }

        // Обработчик события TextChanged для TextBox поиска
        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            // Можно добавить live search здесь, если нужно
            // Но для простоты оставим только по кнопке
        }

        // Ограничение ввода в TextBox поиска
        private void textBoxSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Максимальная длина для поиска - 100 символов (как и для ФИО)
            if (textBox1.Text.Length >= 100 && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
                return;
            }

            // Разрешаем только буквы, пробелы, дефисы и управляющие клавиши
            if (!char.IsLetter(e.KeyChar) &&
                e.KeyChar != ' ' &&
                e.KeyChar != '-' &&
                e.KeyChar != '\'' &&
                !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // Обработчик нажатия Enter в TextBox поиска
        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchPerson();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void SaveDataToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Собираем заголовки из DataGridView
                List<string> headers = new List<string>();
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    headers.Add(column.HeaderText);
                }
                writer.WriteLine(string.Join("|", headers));

                // Собираем данные из строк
                for (int row = 0; row < dataGridView1.Rows.Count - 1; row++) // -1 чтобы исключить новую пустую строку
                {
                    DataGridViewRow dataRow = dataGridView1.Rows[row];
                    List<string> rowData = new List<string>();

                    for (int col = 0; col < dataGridView1.Columns.Count; col++)
                    {
                        string value = dataRow.Cells[col].Value?.ToString()?.Trim() ?? "";

                        // Для столбца ИМТ, если пусто - рассчитываем на лету
                        if (string.IsNullOrEmpty(value) &&
                            dataGridView1.Columns[col].HeaderText.ToLower().Contains("имт"))
                        {
                            // Пытаемся рассчитать ИМТ
                            string weightStr = "";
                            string heightStr = "";

                            // Ищем столбец "Вес"
                            foreach (DataGridViewCell cell in dataRow.Cells)
                            {
                                if (dataGridView1.Columns[cell.ColumnIndex].HeaderText.ToLower() == "вес")
                                {
                                    weightStr = cell.Value?.ToString();
                                    break;
                                }
                            }

                            // Ищем столбец "Рост"
                            foreach (DataGridViewCell cell in dataRow.Cells)
                            {
                                if (dataGridView1.Columns[cell.ColumnIndex].HeaderText.ToLower() == "рост")
                                {
                                    heightStr = cell.Value?.ToString();
                                    break;
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(weightStr) &&
                                !string.IsNullOrWhiteSpace(heightStr) &&
                                double.TryParse(weightStr, out double weight) &&
                                double.TryParse(heightStr, out double height) &&
                                weight >= 30 && weight <= 300 &&
                                height >= 120 && height <= 230)
                            {
                                double heightInMeters = height / 100.0;
                                double bmi = weight / (heightInMeters * heightInMeters);
                                value = Math.Round(bmi, 2).ToString();
                            }
                        }

                        rowData.Add(value);
                    }

                    writer.WriteLine(string.Join("|", rowData));
                }
            }
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем ячейку после редактирования
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                DataGridViewCell cell = row.Cells[e.ColumnIndex];
                string columnName = dataGridView1.Columns[e.ColumnIndex].HeaderText.ToLower();
                string value = cell.Value?.ToString() ?? "";

                bool isValid = true;
                string errorText = "";

                switch (columnName)
                {
                    case "фио":
                    case "имя фамилия":
                    case "имя":
                    case "фамилия":
                        isValid = ValidateFIO(value);
                        errorText = "Некорректное ФИО";
                        break;
                    case "возраст":
                        isValid = ValidateAge(value);
                        errorText = "Некорректный возраст";
                        break;
                    case "рост":
                        isValid = ValidateHeight(value);
                        errorText = "Некорректный рост";
                        break;
                    case "вес":
                        isValid = ValidateWeight(value);
                        errorText = "Некорректный вес";
                        break;
                }

                if (!isValid && !string.IsNullOrWhiteSpace(value))
                {
                    row.ErrorText = string.IsNullOrEmpty(row.ErrorText) ?
                        errorText : row.ErrorText + $"; {errorText}";
                }
            }
        }

        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Обработка ошибок данных
            MessageBox.Show($"Ошибка в данных: {e.Exception.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.ThrowException = false;
        }

        private void DataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            // При добавлении новой строки, если у нее есть вес и рост - рассчитываем ИМТ
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                if (i < dataGridView1.Rows.Count && !dataGridView1.Rows[i].IsNewRow)
                {
                    CalculateBMIForRow(i);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button1.Text = "Сохранить";
            button2.Text = "Удалить";
            button3.Text = "Поиск";
            button4.Text = "Режим редактирования";
            button5.Text = "Режим просмотра";
            button6.Text = "Максимальный рост";
            button4.BackColor = Color.Green;
            button5.BackColor = SystemColors.Control;
            button6.BackColor = SystemColors.Control;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button1.Text = "Сохранение отключено";
            button2.Text = "Удаление отключено";
            button3.Text = "Поиск";
            button4.Text = "Режим редактирования";
            button5.Text = "Режим просмотра";
            button6.Text = "Максимальный рост";
            button5.BackColor = Color.Red;
            button4.BackColor = SystemColors.Control;
            button6.BackColor = SystemColors.Control;
        }
    }
}