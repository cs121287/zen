using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ZenGardenGenerator
{
    /// <summary>
    /// Main form for the Traditional Japanese Zen Garden (Karesansui) Generator
    /// </summary>
    public partial class ZenGardenForm : Form
    {
        #region Private Fields

        // Core components
        private readonly Random random = new();
        private readonly GardenGenerator gardenGenerator;
        private readonly object lockObject = new();

        // UI Components - will be initialized in SetupUserInterface
        private TableLayoutPanel? mainContainer;
        private Panel? gardenPanel;
        private Panel? legendPanel;
        private Panel? principlesPanel;
        private RichTextBox? gardenDisplay;
        private Button? generateButton;
        private Button? saveButton;
        private Label? statusLabel;
        private ProgressBar? progressBar;

        // Configuration constants
        private const int GARDEN_WIDTH = 120;
        private const int GARDEN_HEIGHT = 60;
        private const int MAX_GENERATION_TIME_MS = 30000; // 30 seconds timeout

        // Threading and state management
        private CancellationTokenSource cancellationTokenSource = new();
        private volatile bool isGenerating = false;
        private volatile bool isFormReady = false;
        private bool disposed = false;

        // Current garden data
        private char[,]? currentGarden;
        private Dictionary<char, ZenElement>? elementDictionary;

        // Static readonly arrays for legend display
        private static readonly char[] RockStoneSymbols = ['#', '@', 'o'];
        private static readonly char[] GravelSandSymbols = ['.', '-', '|', '~'];
        private static readonly char[] SpiritualElementSymbols = ['^', '*', '=', '+'];

        #endregion

        #region Constructor and Form Setup

        public ZenGardenForm()
        {
            try
            {
                gardenGenerator = new GardenGenerator(random);
                elementDictionary = gardenGenerator.GetElementDictionary();

                InitializeComponent();
                SetupUserInterface();

                // Enable double buffering for smooth rendering
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                        ControlStyles.UserPaint |
                        ControlStyles.DoubleBuffer |
                        ControlStyles.ResizeRedraw, true);
            }
            catch (Exception ex)
            {
                ShowError("Failed to initialize Zen Garden", ex);
            }
        }

        private void SetupUserInterface()
        {
            try
            {
                CreateMainLayout();
                CreateGardenDisplay();
                CreateLegendPanel();
                CreatePrinciplesPanel();
                CreateControlPanel();
            }
            catch (Exception ex)
            {
                ShowError("Failed to setup user interface", ex);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                isFormReady = true;
                _ = GenerateGardenAsync();
            }
            catch (Exception ex)
            {
                ShowError("Failed to load initial garden", ex);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (!isFormReady)
            {
                isFormReady = true;
                _ = GenerateGardenAsync();
            }
        }

        #endregion

        #region UI Creation Methods

        private void CreateMainLayout()
        {
            mainContainer?.Dispose();

            mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                BackColor = Color.FromArgb(245, 245, 240),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // Configure column styles for responsive layout
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); // Garden area
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Legend area
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Principles area

            // Configure row styles
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 90F)); // Main content
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));  // Controls

            Controls.Add(mainContainer);
        }

        private void CreateGardenDisplay()
        {
            if (mainContainer == null) return;

            gardenPanel?.Dispose();

            gardenPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 248, 245),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                AutoScroll = true
            };

            // Create the main garden display using RichTextBox for color support
            gardenDisplay?.Dispose();
            gardenDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 8F, FontStyle.Regular),
                BackColor = Color.FromArgb(250, 248, 245),
                ForeColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false,
                DetectUrls = false,
                EnableAutoDragDrop = false,
                HideSelection = false
            };

            // Create garden title
            var gardenTitle = new Label
            {
                Text = "🧘 KARESANSUI - TRADITIONAL ZEN GARDEN 🧘",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(240, 240, 235)
            };

            // Create progress bar for generation feedback
            progressBar?.Dispose();
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 10,
                Style = ProgressBarStyle.Continuous,
                Visible = false,
                ForeColor = Color.FromArgb(144, 238, 144)
            };

            gardenPanel.Controls.AddRange([gardenDisplay, progressBar, gardenTitle]);
            mainContainer.Controls.Add(gardenPanel, 0, 0);
        }

        private void CreateLegendPanel()
        {
            if (mainContainer == null) return;

            legendPanel?.Dispose();

            legendPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 246, 243),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                AutoScroll = true
            };

            var legendTitle = new Label
            {
                Text = "🏮 ELEMENTS & MEANINGS",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(139, 69, 19),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(240, 235, 230)
            };

            var legendContent = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(248, 246, 243),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9F),
                DetectUrls = false,
                EnableAutoDragDrop = false
            };

            CreateLegendContent(legendContent);

            legendPanel.Controls.AddRange([legendContent, legendTitle]);
            mainContainer.Controls.Add(legendPanel, 1, 0);
        }

        private void CreatePrinciplesPanel()
        {
            if (mainContainer == null) return;

            principlesPanel?.Dispose();

            principlesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 246, 248),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                AutoScroll = true
            };

            var principlesTitle = new Label
            {
                Text = "☯ ZEN PRINCIPLES",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(230, 235, 240)
            };

            var principlesContent = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(243, 246, 248),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9F),
                DetectUrls = false,
                EnableAutoDragDrop = false
            };

            CreatePrinciplesContent(principlesContent);

            principlesPanel.Controls.AddRange([principlesContent, principlesTitle]);
            mainContainer.Controls.Add(principlesPanel, 2, 0);
        }

        private void CreateControlPanel()
        {
            if (mainContainer == null) return;

            var controlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(235, 235, 230),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10, 5, 10, 10)
            };

            // Generate Garden button
            generateButton?.Dispose();
            generateButton = new Button
            {
                Text = "🌸 Generate New Garden",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(180, 35),
                Location = new Point(20, 15),
                BackColor = Color.FromArgb(144, 238, 144),
                ForeColor = Color.FromArgb(0, 100, 0),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            generateButton.FlatAppearance.BorderColor = Color.FromArgb(34, 139, 34);
            generateButton.Click += GenerateButton_Click;

            // Save Garden button
            saveButton?.Dispose();
            saveButton = new Button
            {
                Text = "💾 Save Garden",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(140, 35),
                Location = new Point(220, 15),
                BackColor = Color.FromArgb(173, 216, 230),
                ForeColor = Color.FromArgb(0, 0, 139),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            saveButton.FlatAppearance.BorderColor = Color.FromArgb(70, 130, 180);
            saveButton.Click += SaveButton_Click;

            // Status label with current timestamp
            statusLabel?.Dispose();
            statusLabel = new Label
            {
                Text = "Generated on: 2025-06-22 21:25:56 UTC for user: cs121287",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(105, 105, 105),
                Location = new Point(380, 20),
                Size = new Size(400, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            controlPanel.Controls.AddRange([generateButton, saveButton, statusLabel]);
            mainContainer.Controls.Add(controlPanel, 0, 1);
            mainContainer.SetColumnSpan(controlPanel, 3);
        }

        #endregion

        #region Garden Generation Engine

        private async Task GenerateGardenAsync()
        {
            if (!isFormReady || isGenerating) return;

            try
            {
                lock (lockObject)
                {
                    if (isGenerating) return;
                    isGenerating = true;
                }

                // Cancel any existing generation
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = new CancellationTokenSource();

                SetUIGenerating(true);

                using var timeoutCts = new CancellationTokenSource(MAX_GENERATION_TIME_MS);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationTokenSource.Token, timeoutCts.Token);

                // Create progress reporter
                var progress = new Progress<int>(UpdateProgress);

                // Generate the entire garden before displaying
                currentGarden = await gardenGenerator.GenerateGardenAsync(
                    GARDEN_WIDTH, GARDEN_HEIGHT, progress, combinedCts.Token);

                if (!combinedCts.Token.IsCancellationRequested && !IsDisposed && isFormReady)
                {
                    await ApplyGardenToUI(currentGarden, combinedCts.Token);
                    UpdateStatusLabel();
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, no error message needed
            }
            catch (Exception ex)
            {
                ShowError("Failed to generate garden", ex);
            }
            finally
            {
                SetUIGenerating(false);
                lock (lockObject)
                {
                    isGenerating = false;
                }
            }
        }

        private async Task ApplyGardenToUI(char[,] garden, CancellationToken cancellationToken)
        {
            if (!isFormReady || gardenDisplay == null || gardenDisplay.IsDisposed) return;

            await Task.Run(() =>
            {
                SafeInvoke(() =>
                {
                    if (isFormReady && gardenDisplay != null && !gardenDisplay.IsDisposed && gardenDisplay.IsHandleCreated)
                    {
                        // Convert garden array to string
                        var gardenText = ConvertGardenToString(garden);

                        gardenDisplay.Clear();
                        gardenDisplay.Text = gardenText;
                        ApplyColorFormatting();
                    }
                });
            }, cancellationToken);
        }

        private string ConvertGardenToString(char[,] garden)
        {
            var gardenText = new StringBuilder();
            int height = garden.GetLength(0);
            int width = garden.GetLength(1);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    gardenText.Append(garden[row, col]);
                }
                if (row < height - 1)
                {
                    gardenText.AppendLine();
                }
            }

            return gardenText.ToString();
        }

        private void ApplyColorFormatting()
        {
            try
            {
                if (!isFormReady || gardenDisplay == null || gardenDisplay.IsDisposed || !gardenDisplay.IsHandleCreated)
                    return;

                if (elementDictionary == null) return;

                // Set default color
                gardenDisplay.SelectAll();
                gardenDisplay.SelectionColor = Color.FromArgb(60, 60, 60);
                gardenDisplay.SelectionStart = 0;

                // Apply colors to individual elements
                string text = gardenDisplay.Text;
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (elementDictionary.TryGetValue(c, out ZenElement? element) && element != null)
                    {
                        gardenDisplay.SelectionStart = i;
                        gardenDisplay.SelectionLength = 1;
                        gardenDisplay.SelectionColor = element.Color;
                    }
                }

                // Reset selection
                gardenDisplay.SelectionStart = 0;
                gardenDisplay.SelectionLength = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Color formatting error: {ex.Message}");
            }
        }

        #endregion

        #region UI Content Creation

        private void CreateLegendContent(RichTextBox rtb)
        {
            try
            {
                if (rtb == null || rtb.IsDisposed || elementDictionary == null) return;

                rtb.Clear();

                AddLegendSection(rtb, "ROCKS AND STONES:", RockStoneSymbols);
                AddLegendSection(rtb, "GRAVEL & SAND:", GravelSandSymbols);
                AddLegendSection(rtb, "SPIRITUAL ELEMENTS:", SpiritualElementSymbols);
            }
            catch (Exception ex)
            {
                ShowError("Failed to create legend content", ex);
            }
        }

        private void AddLegendSection(RichTextBox rtb, string title, char[] symbols)
        {
            try
            {
                rtb.SelectionFont = new Font("Segoe UI", 10F, FontStyle.Bold);
                rtb.SelectionColor = Color.FromArgb(139, 69, 19);
                rtb.AppendText($"{title}\n");

                foreach (char symbol in symbols)
                {
                    if (elementDictionary != null && elementDictionary.TryGetValue(symbol, out ZenElement? element) && element != null)
                    {
                        rtb.SelectionFont = new Font("Consolas", 11F, FontStyle.Bold);
                        rtb.SelectionColor = element.Color;
                        rtb.AppendText($"{element.Symbol} ");

                        rtb.SelectionFont = new Font("Segoe UI", 9F, FontStyle.Regular);
                        rtb.SelectionColor = Color.Black;
                        rtb.AppendText($"= {element.Name}\n");

                        rtb.SelectionFont = new Font("Segoe UI", 8F, FontStyle.Italic);
                        rtb.SelectionColor = Color.FromArgb(105, 105, 105);
                        rtb.AppendText($"   {element.Meaning}\n\n");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Legend section error: {ex.Message}");
            }
        }

        private void CreatePrinciplesContent(RichTextBox rtb)
        {
            try
            {
                if (rtb == null || rtb.IsDisposed) return;

                rtb.Clear();

                (string title, string description)[] principles = [
                    ("🌸 AUSTERITY", "Minimal elements with maximum meaning. Each symbol represents profound natural forces."),
                    ("🎋 SIMPLICITY", "Clean lines and uncluttered spaces promote mental clarity and peace."),
                    ("🍃 NATURALNESS", "Organic placement mimics nature's own asymmetrical beauty."),
                    ("⚖️ ASYMMETRY", "Avoiding perfect balance creates natural, harmonious arrangements."),
                    ("🌙 MYSTERY", "Subtle suggestions rather than literal representation invite contemplation."),
                    ("🧘 STILLNESS", "Promotes deep meditation and connection with inner peace."),
                    ("🌊 IMPERMANENCE", "Raked patterns remind us that all things change and flow."),
                    ("🌿 HARMONY", "Each element works together to create unified tranquility.")
                ];

                foreach (var (title, description) in principles)
                {
                    rtb.SelectionFont = new Font("Segoe UI", 10F, FontStyle.Bold);
                    rtb.SelectionColor = Color.FromArgb(25, 25, 112);
                    rtb.AppendText($"{title}\n");

                    rtb.SelectionFont = new Font("Segoe UI", 9F, FontStyle.Regular);
                    rtb.SelectionColor = Color.Black;
                    rtb.AppendText($"{description}\n\n");
                }

                rtb.SelectionFont = new Font("Segoe UI", 10F, FontStyle.Bold);
                rtb.SelectionColor = Color.FromArgb(139, 0, 0);
                rtb.AppendText("MEDITATION GUIDE:\n");

                rtb.SelectionFont = new Font("Segoe UI", 9F, FontStyle.Italic);
                rtb.SelectionColor = Color.FromArgb(105, 105, 105);
                rtb.AppendText("Focus on the arrangement before you. Let your eyes move naturally across the garden, noticing how each element relates to the others. The rocks represent stability in change, while the raked patterns show the eternal flow of time and water. Allow your mind to find the same stillness reflected in this digital representation of ancient wisdom.");
            }
            catch (Exception ex)
            {
                ShowError("Failed to create principles content", ex);
            }
        }

        #endregion

        #region Event Handlers

        private async void GenerateButton_Click(object? sender, EventArgs e)
        {
            await GenerateGardenAsync();
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!isFormReady || gardenDisplay?.Text == null || string.IsNullOrEmpty(gardenDisplay.Text))
                {
                    MessageBox.Show("No garden to save. Please generate a garden first.", "Save Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var saveDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"ZenGarden_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    Title = "Save Zen Garden"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    SetUIGenerating(true);

                    await Task.Run(() =>
                    {
                        var content = CreateSaveContent();
                        File.WriteAllText(saveDialog.FileName, content, Encoding.UTF8);
                    });

                    MessageBox.Show($"Zen garden saved successfully to:\n{saveDialog.FileName}",
                        "Save Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied. Please choose a different location or run as administrator.",
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("The specified directory was not found. Please choose a valid location.",
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"File I/O error: {ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                ShowError("Failed to save garden", ex);
            }
            finally
            {
                SetUIGenerating(false);
            }
        }

        #endregion

        #region Helper Methods

        private string CreateSaveContent()
        {
            var content = new StringBuilder();
            content.AppendLine("TRADITIONAL JAPANESE ZEN GARDEN (KARESANSUI)");
            content.AppendLine("=" + new string('=', 50));
            content.AppendLine();
            content.AppendLine(gardenDisplay?.Text ?? "");
            content.AppendLine();
            content.AppendLine("ELEMENTS LEGEND:");
            content.AppendLine("=" + new string('=', 20));

            if (elementDictionary != null)
            {
                foreach (var element in elementDictionary.Values.OrderBy(e => e.Symbol))
                {
                    content.AppendLine($"{element.Symbol} = {element.Name} ({element.Meaning})");
                }
            }

            content.AppendLine();
            content.AppendLine("ZEN PRINCIPLES:");
            content.AppendLine("=" + new string('=', 20));
            content.AppendLine("Austerity: Minimal elements with maximum meaning");
            content.AppendLine("Simplicity: Clean lines promoting mental clarity");
            content.AppendLine("Naturalness: Organic asymmetrical beauty");
            content.AppendLine("Mystery: Subtle suggestions inviting contemplation");
            content.AppendLine("Stillness: Deep meditation and inner peace");
            content.AppendLine("Impermanence: Eternal flow of time and change");
            content.AppendLine("Harmony: Unified tranquility of all elements");
            content.AppendLine();
            content.AppendLine("AUTHENTIC JAPANESE GARDEN COLOR SCHEME:");
            content.AppendLine("=" + new string('=', 45));
            content.AppendLine("Colors inspired by traditional Japanese garden imagery:");
            content.AppendLine("# Large Rocks - Deep weathered stone gray (85, 85, 85)");
            content.AppendLine("@ Medium Rocks - Natural stone gray (105, 105, 105)");
            content.AppendLine("o Small Stones - Light stone gray (128, 128, 128)");
            content.AppendLine(". Fine Gravel - Warm gravel beige (230, 220, 200)");
            content.AppendLine("- Horizontal Raked - Subtle blue-gray patterns (180, 190, 200)");
            content.AppendLine("| Vertical Raked - Flow pattern blue-gray (160, 175, 190)");
            content.AppendLine("~ Curved Raked - Flowing wave blue-gray (140, 160, 180)");
            content.AppendLine("^ Moss - Rich natural moss green (85, 120, 60)");
            content.AppendLine("* Stone Lantern - Warm golden lantern glow (255, 200, 100)");
            content.AppendLine("= Bridge/Path - Natural wood bridge brown (160, 120, 80)");
            content.AppendLine("+ Water Feature - Beautiful Japanese pond blue (70, 130, 180)");
            content.AppendLine();
            content.AppendLine("ADVANCED GENERATION RULES & AUTHENTIC JAPANESE PRINCIPLES:");
            content.AppendLine("=" + new string('=', 65));
            content.AppendLine("Phase-based generation: Terrain → Water → Infrastructure → Gravel → Flow → Decoration");
            content.AppendLine("Zone-based placement: Focal points, flow areas, gravel gardens, edges, corners");
            content.AppendLine("ASCII visual density hierarchy: # (darkest) to . (lightest)");
            content.AppendLine("Authentic element limits and placement rules");
            content.AppendLine("Natural water systems with winding rivers and contained ponds");
            content.AppendLine("Single bridge per garden crossing water features");
            content.AppendLine("Protected gravel garden zones for contemplation");
            content.AppendLine("Moss growth near rocks, water, and edges mimicking nature");
            content.AppendLine("Stone lanterns positioned for maximum spiritual impact");
            content.AppendLine("Flow patterns enhanced around obstacles");
            content.AppendLine("Visual density optimization for proper ASCII art contrast");
            content.AppendLine("Colors based on authentic Japanese garden photography");
            content.AppendLine();
            content.AppendLine("Generated on: 2025-06-22 21:25:56 UTC for user: cs121287");
            content.AppendLine("Created with Advanced Traditional Japanese Zen Garden Generator");
            content.AppendLine("Each garden follows authentic Karesansui principles with modern algorithms");

            return content.ToString();
        }

        private void UpdateProgress(int progress)
        {
            SafeInvoke(() =>
            {
                if (progressBar != null && !progressBar.IsDisposed && progressBar.IsHandleCreated)
                {
                    progressBar.Value = Math.Min(progress, 100);
                }
            });
        }

        private void SetUIGenerating(bool generating)
        {
            SafeInvoke(() =>
            {
                if (generateButton != null && !generateButton.IsDisposed && generateButton.IsHandleCreated)
                {
                    generateButton.Enabled = !generating;
                    generateButton.Text = generating ? "🔄 Generating..." : "🌸 Generate New Garden";
                }

                if (saveButton != null && !saveButton.IsDisposed && saveButton.IsHandleCreated)
                {
                    saveButton.Enabled = !generating;
                }

                if (progressBar != null && !progressBar.IsDisposed && progressBar.IsHandleCreated)
                {
                    progressBar.Visible = generating;
                    if (generating)
                    {
                        progressBar.Value = 0;
                    }
                }
            });
        }

        private void UpdateStatusLabel()
        {
            SafeInvoke(() =>
            {
                if (statusLabel != null && !statusLabel.IsDisposed && statusLabel.IsHandleCreated)
                {
                    statusLabel.Text = "Generated on: 2025-06-22 21:25:56 UTC for user: cs121287";
                }
            });
        }

        private void SafeInvoke(Action action)
        {
            try
            {
                if (!isFormReady) return;

                if (InvokeRequired)
                {
                    try
                    {
                        BeginInvoke(action);
                    }
                    catch (InvalidOperationException)
                    {
                        // Handle may not be created yet, ignore
                    }
                }
                else
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SafeInvoke error: {ex.Message}");
            }
        }

        private void ShowError(string message, Exception ex)
        {
            SafeInvoke(() =>
            {
                var errorMessage = $"{message}\n\nError: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
                }

                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error: {message} - {ex}");
            });
        }

        #endregion

        #region Resource Cleanup

        private void CleanupResources()
        {
            try
            {
                isFormReady = false;

                cancellationTokenSource?.Cancel();
                Thread.Sleep(100);

                cancellationTokenSource?.Dispose();

                // Dispose UI components
                gardenDisplay?.Dispose();
                gardenPanel?.Dispose();
                legendPanel?.Dispose();
                principlesPanel?.Dispose();
                generateButton?.Dispose();
                saveButton?.Dispose();
                statusLabel?.Dispose();
                progressBar?.Dispose();
                mainContainer?.Dispose();

                // Dispose zen elements
                if (elementDictionary != null)
                {
                    foreach (var element in elementDictionary.Values)
                    {
                        element?.Dispose();
                    }
                    elementDictionary.Clear();
                }

                disposed = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cleanup error: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                CleanupResources();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}