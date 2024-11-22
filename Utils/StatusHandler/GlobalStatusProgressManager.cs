using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.ComponentModel;
using System.Drawing;

namespace Utils
{
    public class GlobalStatusProgressManager : IGlobalStatusProgressHandler
    {
        //private static readonly ILog LOG = LogManager.GetLogger(typeof(GlobalStatusProgressManager));
        private static readonly dynamic LOG = new object();//Replace for a real log system

        private readonly ToolTip toolTip = new ToolTip();
        private readonly BindingList<StatusProgress> currentStatus = new BindingList<StatusProgress>();
        private Action<string> textSetter;
        private Control control;
        private System.Timers.Timer toolTipCloser;
        private string defaultText = "Ready";
        private StatusProgress readyStatus;

        public void SubscribeToStatusProgress(ToolStripStatusLabel toolTipControl, string defaultText = "Ready", bool showBalloonIcon = false)
        {
            this.defaultText = defaultText;
            readyStatus = new StatusProgress(defaultText);
            control = toolTipControl.Owner;

            textSetter = p =>
            {
                try
                {
                    if (toolTipControl.Owner != null && toolTipControl.Owner.IsHandleCreated)
                    {
                        toolTipControl.Owner.Invoke(new Action(() =>
                        {
                            if (showBalloonIcon)
                            {
                                string balloonIcon = currentStatus.Count > 1 ? "💬" : "💭";
                                toolTipControl.Text = $"{balloonIcon} {p}";
                            }
                            else
                            {
                                toolTipControl.Text = p;
                            }
                        }));
                    }
                    else
                    {
                        // Se o controle ainda não estiver pronto, define o texto diretamente
                        if (showBalloonIcon)
                        {
                            string balloonIcon = currentStatus.Count > 1 ? "💬" : "💭";
                            toolTipControl.Text = $"{balloonIcon} {p}";
                        }
                        else
                        {
                            toolTipControl.Text = p;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error(ex);
                }
            };

            textSetter.Invoke(defaultText);

            toolTipControl.MouseUp += ToolTipControl_MouseUp;

            Initialize();
        }

        public void SubscribeToStatusProgress(Control control, string defaultText = "Ready")
        {
            this.defaultText = defaultText;
            readyStatus = new StatusProgress(defaultText);
            control.MouseUp += ToolTipControl_MouseUp;
            this.control = control;
            textSetter = p => control.Invoke(new Action(() => control.Text = p));

            Initialize();
        }

        private void Initialize()
        {
            toolTip.IsBalloon = true;
            toolTipCloser = new System.Timers.Timer(2000) { AutoReset = false };
            toolTipCloser.Elapsed += ToolTipCloser_Elapsed;
            currentStatus.ListChanged += CurrentStatus_ListChanged;
        }

        private void CurrentStatus_ListChanged(object sender, ListChangedEventArgs e)
        {
            try
            {
                StatusProgress status = null;
                if (e.ListChangedType == ListChangedType.ItemChanged || e.ListChangedType == ListChangedType.ItemAdded)
                {
                    status = currentStatus[e.NewIndex];
                    if (status.Rate >= status.endRate)
                    {
                        lock (currentStatus)
                            currentStatus.Remove(status);
                        return;
                    }
                }
                else if (e.ListChangedType == ListChangedType.ItemDeleted)
                {
                    status = currentStatus.LastOrDefault() ?? readyStatus;
                }

                UpdateStatusText(status);
            }
            catch (Exception ex)
            {
                LOG.Error(ex);
            }
        }

        private void UpdateStatusText(StatusProgress status)
        {
            string message = status?.GetFormattedStatus() ?? readyStatus.Message;
            textSetter?.Invoke(message);
        }

        private void ToolTipCloser_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (tooltipForm != null && tooltipForm.InvokeRequired)
            {
                tooltipForm.Invoke(new Action(() =>
                {
                    tooltipForm.Close();
                    tooltipForm = null;
                }));
            }
            else
            {
                tooltipForm?.Close();
                tooltipForm = null;
            }
        }

        private TooltipForm tooltipForm;
        private void ToolTipControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (control != null)
            {
                tooltipForm?.Close();

                string separator = new string('-', 30);

                var statusText = currentStatus.Any()
                    ? string.Join(Environment.NewLine + separator + Environment.NewLine, currentStatus.Select(s => WrapText(s.GetFormattedStatus(), 50)))
                    : defaultText;

                tooltipForm = new TooltipForm();
                tooltipForm.SetText(statusText);

                var screenPoint = control.PointToScreen(new Point(e.X, e.Y));
                tooltipForm.Location = new Point(screenPoint.X, screenPoint.Y - tooltipForm.Height - 10);
                tooltipForm.Show();

                toolTipCloser.Stop();
                toolTipCloser.Start();
            }
        }

        private string WrapText(string text, int maxLineLength)
        {
            var words = text.Split(' ');
            var result = new StringBuilder();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 > maxLineLength)
                {
                    result.AppendLine(currentLine.ToString());
                    currentLine.Clear();
                }
                if (currentLine.Length > 0)
                    currentLine.Append(" ");

                currentLine.Append(word);
            }
            result.AppendLine(currentLine.ToString());

            return result.ToString();
        }

        public void HandleThisEvent(StatusProgress statusProgress)
        {
            lock (currentStatus)
            {
                if (!currentStatus.Contains(statusProgress))
                {
                    currentStatus.Add(statusProgress);
                }
            }
        }

        private void SafeInvoke<T>(T invokableObj, Action<T> call) where T : ISynchronizeInvoke
        {
            if (invokableObj.InvokeRequired)
            {
                IAsyncResult result = invokableObj.BeginInvoke(call, new object[] { invokableObj });
                _ = invokableObj.EndInvoke(result);
            }
            else
            {
                call(invokableObj);
            }
        }
    }

    public interface IGlobalStatusProgressHandler
    {
        void HandleThisEvent(StatusProgress statusProgress);
        void SubscribeToStatusProgress(ToolStripStatusLabel toolTipControl, string defaultText = "Ready", bool showBalloonIcon = false);
        void SubscribeToStatusProgress(Control control, string defaultText = "Ready");
    }

    public class TooltipForm : Form
    {
        private Label label;

        public TooltipForm()
        {
            // Configuração do Form como Tooltip
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.Padding = new Padding(10);
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            this.Paint += TooltipForm_Paint;

            // Configuração do Label interno
            label = new Label();
            label.AutoSize = true;
            label.MaximumSize = new Size(300, 0);
            label.TextAlign = ContentAlignment.TopLeft;
            label.ForeColor = Color.White;
            label.BackColor = Color.Transparent;
            label.Font = new Font("Segoe UI", 9);
            this.Controls.Add(label);
        }

        public void SetText(string text)
        {
            label.Text = text;

            this.ClientSize = label.PreferredSize;
        }

        private void TooltipForm_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Gray, 1))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1));
            }
        }
    }
}