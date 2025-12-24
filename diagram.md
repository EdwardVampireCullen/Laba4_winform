```mermaid
classDiagram
    direction LR
    
    %% Основные классы проекта
    class Form2 {
        -OpenFileDialog openFileDialog1
        -SaveFileDialog saveFileDialog1
        -DataGridViewTextBoxEditingControl currentEditingControl
        -bool isCalculatingBMI
        +Form2()
        -InitializeOpenFileDialog()
        -InitializeSaveFileDialog()
        -Form2_Shown()
        -OpenAndLoadFile()
        -LoadFile(string)
        -ConfigureDataGridView()
        -DataGridView1_EditingControlShowing()
        -EditingControl_KeyPress()
        -DataGridView1_CellValueChanged()
        -CalculateBMIForRow(int)
        -CalculateAllBMI()
        -ValidateAllData() bool
        -ValidateFIO(string) bool
        -ValidateAge(string) bool
        -ValidateHeight(string) bool
        -ValidateWeight(string) bool
        +button1_Click()
        +button2_Click()
        +button3_Click()
        +button6_Click()
        -SearchPerson()
        -FindPersonWithMaxHeight()
        -GetFIOFromRow(DataGridViewRow) string
        -GetHeightFromRow(DataGridViewRow) string
        -SaveDataToFile(string)
    }
    
    %% Классы .NET Framework, используемые в проекте
    class Form {
        <<System.Windows.Forms>>
    }
    
    class OpenFileDialog {
        <<System.Windows.Forms>>
        +Filter
        +FilterIndex
        +RestoreDirectory
        +Title
        +ShowDialog() DialogResult
        +FileName
    }
    
    class SaveFileDialog {
        <<System.Windows.Forms>>
        +Filter
        +FilterIndex
        +RestoreDirectory
        +Title
        +DefaultExt
        +ShowDialog() DialogResult
        +FileName
    }
    
    class DataGridView {
        <<System.Windows.Forms>>
        +Columns
        +Rows
        +SelectedRows
        +ReadOnly
        +AllowUserToAddRows
        +SelectionMode
        +MultiSelect
        +ClearSelection()
        +Rows_Add()
        +Rows_Remove()
    }
    
    class DataGridViewRow {
        <<System.Windows.Forms>>
        +Cells
        +IsNewRow
        +ErrorText
        +Selected
    }
    
    class DataGridViewTextBoxEditingControl {
        <<System.Windows.Forms>>
        +KeyPress event
    }
    
    class StreamWriter {
        <<System.IO>>
        +WriteLine(string)
    }
    
    class Regex {
        <<System.Text.RegularExpressions>>
        +IsMatch(string) bool
    }
    
    %% Отношения наследования
    Form2 --|> Form
    DataGridViewTextBoxEditingControl --|> Control
    
    %% Ассоциации (композиция/агрегация)
    Form2 "1" *-- "1" OpenFileDialog : содержит
    Form2 "1" *-- "1" SaveFileDialog : содержит
    Form2 "1" *-- "1" DataGridView : содержит
    Form2 "1" *-- "0..1" DataGridViewTextBoxEditingControl : содержит
    Form2 "1" *-- "1" StreamWriter : использует при сохранении
    Form2 "1" *-- "1" Regex : использует для валидации
    
    DataGridView "1" *-- "n" DataGridViewRow : содержит строки
    
    %% Зависимости (use-a relationships)
    Form2 ..> MessageBox : вызывает для сообщений
    Form2 ..> File : использует для работы с файлами
    Form2 ..> Encoding : использует UTF8
    Form2 ..> Math : использует для округления ИМТ
    
    note for Form2 "Отвечает за:\n1. Загрузку/сохранение данных\n2. Валидацию ввода\n3. Расчет ИМТ\n4. Управление UI"
