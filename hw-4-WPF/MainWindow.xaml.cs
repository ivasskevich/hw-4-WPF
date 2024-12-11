using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace hw_4_WPF
{
    public partial class MainWindow : Window
    {
        private string activeFile = null;
        private double currentZoomLevel = 1.0;
        private bool unsavedChangesExist = false;

        public MainWindow()
        {
            InitializeComponent();
            editorTextBox.TextChanged += (s, e) => unsavedChangesExist = true;
        }

        private void CreateNewFile(object sender, RoutedEventArgs e)
        {
            if (RequestToSaveChanges())
            {
                editorTextBox.Clear();
                activeFile = null;
                Title = "Notepad+ - Untitled";
                unsavedChangesExist = false;
            }
        }

        private void LoadFile(object sender, RoutedEventArgs e)
        {
            if (!RequestToSaveChanges()) return;

            OpenFileDialog openDialog = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*" };
            if (openDialog.ShowDialog() == true)
            {
                activeFile = openDialog.FileName;
                editorTextBox.Text = File.ReadAllText(activeFile);
                Title = $"Notepad+ - {Path.GetFileName(activeFile)}";
                unsavedChangesExist = false;
            }
        }

        private void SaveCurrentFile(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(activeFile))
                SaveFileAsNew(sender, e);
            else
                SaveFileContent(activeFile);
        }

        private void SaveFileAsNew(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*" };
            if (saveDialog.ShowDialog() == true)
            {
                SaveFileContent(saveDialog.FileName);
            }
        }

        private void SaveFileContent(string filePath)
        {
            File.WriteAllText(filePath, editorTextBox.Text);
            activeFile = filePath;
            unsavedChangesExist = false;
            Title = $"Notepad+ - {Path.GetFileName(filePath)}";
        }

        private bool RequestToSaveChanges()
        {
            if (!unsavedChangesExist) return true;

            var userResponse = MessageBox.Show("Do you want to save changes?", "Notepad+", MessageBoxButton.YesNoCancel);
            if (userResponse == MessageBoxResult.Yes)
                SaveCurrentFile(this, null);
            return userResponse != MessageBoxResult.Cancel;
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            if (RequestToSaveChanges())
                Application.Current.Shutdown();
        }

        private void UndoAction(object sender, RoutedEventArgs e) => editorTextBox.Undo();
        private void CutText(object sender, RoutedEventArgs e) => editorTextBox.Cut();
        private void CopyText(object sender, RoutedEventArgs e) => editorTextBox.Copy();
        private void PasteText(object sender, RoutedEventArgs e) => editorTextBox.Paste();
        private void DeleteSelectedText(object sender, RoutedEventArgs e) => editorTextBox.SelectedText = string.Empty;
        private void SelectAllText(object sender, RoutedEventArgs e) => editorTextBox.SelectAll();

        private void InsertCurrentDateTime(object sender, RoutedEventArgs e)
        {
            editorTextBox.SelectedText = DateTime.Now.ToString();
        }

        private void ZoomIn(object sender, RoutedEventArgs e) => AdjustZoomLevel(0.1);
        private void ZoomOut(object sender, RoutedEventArgs e) => AdjustZoomLevel(-0.1);
        private void ResetZoom(object sender, RoutedEventArgs e) => AdjustZoomLevel(1.0 - currentZoomLevel);

        private void AdjustZoomLevel(double adjustment)
        {
            currentZoomLevel = Math.Max(0.5, currentZoomLevel + adjustment);
            editorTextBox.FontSize = 12 * currentZoomLevel;
        }

        private void ToggleWordWrap(object sender, RoutedEventArgs e)
        {
            editorTextBox.TextWrapping = editorTextBox.TextWrapping == TextWrapping.Wrap ? TextWrapping.NoWrap : TextWrapping.Wrap;
        }

        private void ToggleStatusBar(object sender, RoutedEventArgs e)
        {
            statusTextBlock.Visibility = statusTextBlock.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void FindText(object sender, RoutedEventArgs e)
        {
            string searchQuery = Microsoft.VisualBasic.Interaction.InputBox("Enter text to find:", "Find");
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                int matchIndex = editorTextBox.Text.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase);
                if (matchIndex != -1)
                {
                    editorTextBox.Select(matchIndex, searchQuery.Length);
                    editorTextBox.Focus();
                }
                else
                {
                    MessageBox.Show("Text not found.", "Find");
                }
            }
        }

        private void ReplaceText(object sender, RoutedEventArgs e)
        {
            string searchFor = Microsoft.VisualBasic.Interaction.InputBox("Enter text to replace:", "Replace");
            string replaceWith = Microsoft.VisualBasic.Interaction.InputBox("Enter replacement text:", "Replace");
            editorTextBox.Text = editorTextBox.Text.Replace(searchFor, replaceWith);
        }

        private void GoToLine(object sender, RoutedEventArgs e)
        {
            string lineInput = Microsoft.VisualBasic.Interaction.InputBox("Enter line number:", "Go To Line");
            if (int.TryParse(lineInput, out int line))
            {
                editorTextBox.ScrollToLine(line - 1);
            }
        }

        private void PrintDocument(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
                printDialog.PrintVisual(editorTextBox, "Printing Document");
        }
    }
}
