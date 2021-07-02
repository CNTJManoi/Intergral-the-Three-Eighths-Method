using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using AngouriMath;

namespace calculateIntegral
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Thread _calc;
        private Thread _errorRateThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Evaluate_Click(object sender, RoutedEventArgs e)
        {
            if (_calc is null)
            {
                if (_errorRateThread is not null)
                    if (_errorRateThread.IsAlive)
                    {
                        _errorRateThread.Abort();
                        _errorRateThread = null;
                    }

                string f;
                float b, a;
                float eps;
                try
                {
                    f = FormulaTextBox.Text;
                    b = float.Parse(BTextBox.Text.Replace('.', ','));
                    a = float.Parse(ATextBox.Text.Replace('.', ','));
                    if (Regex.IsMatch(EpsTextBox.Text.Replace('.', ','), @"0\,[0]*[1]{1}$"))
                    {
                        eps = float.Parse(EpsTextBox.Text.Replace('.', ','));
                    }
                    else
                    {
                        MessageBox.Show("Введите корректно точность вычисления!");
                        return;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Введите корректно необходимые значения!");
                    return;
                }

                //if (n < 1)
                //{
                //    MessageBox.Show("Введите n больше 1!");
                //    return;
                //}
                //if (n % 3 != 0)
                //{
                //    MessageBox.Show("Введите n кратное 3!");
                //    return;
                //}

                _calc = new Thread(() => CalcThread(f, b, a, eps));
                Pg1.Value = 0;
                _calc.Start();
                Abort.IsEnabled = true;
                ResultTextBox.Text = "";
                ErrorResultBox.Text = "";
            }
            else
            {
                MessageBox.Show("Вычисление выполняется");
            }
        }

        private void CalcThread(string f, float b, float a, float eps = 0.00001f)
        {
            float res = 1;
            float finaly = 0;
            int n = 3;
            while (eps < res)
            {
                float h = (b - a) / n;
                float progress = 100f / n;
                float? sum = 0;
                for (int i = 0; i <= n; i++)
                {
                    float xi = a + h * i;
                    if (i == 0 || i == n) sum += CalcFunc(f, xi);
                    else if (i % 3 == 0) sum += 2 * CalcFunc(f, xi);
                    else sum += 3f * CalcFunc(f, xi);
                    Pg1.Dispatcher.Invoke(() => Pg1.Value += progress);
                    if (sum == null)
                    {
                        _calc = null;
                        MessageBox.Show("Корректно введите функцию");
                        return;
                    }
                }

                float fin1 = (float)sum * ((3f * h) / 8f);
                finaly = fin1;
                float h2 = (b - a) / (n / 2f);
                float? sum2 = 0;
                for (int i = 0; i <= n / 2f; i++)
                {
                    float xi2 = a + i * h2;
                    if (i == 0 || i == n / 2) sum2 += CalcFunc(f, xi2);
                    else if (i % 3 == 0) sum2 += 2 * CalcFunc(f, xi2);
                    else sum2 += 3 * CalcFunc(f, xi2);
                }

                float fin2 = (float)(sum2 * (3f * h2 / 8f));
                res = Math.Abs(fin1 - fin2) * (1f / 15f);
                n += 3;
            }
            Dispatcher.Invoke(() =>
            {
                ResultTextBox.Text = finaly.ToString(CultureInfo.CurrentCulture);
                ErrorResultBox.Text = res.ToString(CultureInfo.CurrentCulture);
            });
            Dispatcher.Invoke(() => Abort.IsEnabled = false);
            _calc = null;
        }

        private float? CalcFunc(string f, float x)
        {
            float res;
            try
            {
                Entity expr = f;
                res = (float) expr.Substitute("x", x).EvalNumerical();
            }
            catch (Exception)
            {
                Dispatcher.Invoke(() => Abort.IsEnabled = false);
                return null;
            }

            return res;
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            _calc.Abort();
            _calc = null;
            Pg1.Value = 0;
            Abort.IsEnabled = false;
        }

        private void infoOpen_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow info = new InfoWindow();
            info.Show();
        }

        private void InstructionsOpen_Click(object sender, RoutedEventArgs e)
        {
            InstrictionsWindow ins = new InstrictionsWindow();
            ins.Show();
        }
    }
}