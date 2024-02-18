using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UnitConverter
{
    public class TailwindWorker : BackgroundService, IDisposable
    {
        private bool? _isProcessRunning = false;
        private Process? _tailwindProcess;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                if (_isProcessRunning.HasValue && !_isProcessRunning.Value && !IsTailwindProcessRunning())
                {
                    StartTailwindProcess();
                }
                

                if(IsTailwindProcessRunning() && _tailwindProcess == null)
                {
                    _tailwindProcess = Process.GetProcessesByName("tailwindcss").FirstOrDefault();
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private void StartTailwindProcess()
        {
            _tailwindProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "tailwindcss.exe",
                Arguments = @"--config .\tailwind.config.js -i .\Styles\tailwind-app.css -o .\wwwroot\tailwind-app.css --watch",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            _isProcessRunning = true;
        }

        private bool IsTailwindProcessRunning()
        {
            Process[] processes = Process.GetProcessesByName("tailwindcss");
            return processes.Length > 0;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {

            if (_tailwindProcess != null && !_tailwindProcess.HasExited)
            {
                _tailwindProcess.Kill();
                _tailwindProcess.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _tailwindProcess?.Dispose();
        }
    }
}