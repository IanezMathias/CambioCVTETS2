using System;
using System.Windows;
using System.Windows.Forms;


namespace YoutubeETS2CVT
{
    /// <summary>
    /// SE INCREVE LA PESSOAL:  https://www.youtube.com/channel/UCE3OLr2HlIngcuKkVxt2_5A
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Memoria
        ProcessMemory mem;
        int mem_rpm = 0x0;
        int mem_temp_motor = 0x0;
        int mem_temp_oleo = 0x0;
        int mem_hodometro = 0x0;
        int mem_combustivel = 0x0;
        int mem_velocidade = 0x0;
        int mem_cambio = 0x0;
        #endregion

        #region Cambio
        float cambio;
        float rpm_corrente = 0;
        float max_cambio = 30;
        float rpm_alvo = 1500;
        float rpm_divergente = 0;
        float ajuste_deDivergencia = 100;
        float indice_ajuste = 0;
        float indice_max = 4f;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel.MainViewModel();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            mem = new ProcessMemory("eurotrucks2");
            if (mem.StartProcess())
            {
                Iniciar();
            }
        }

        private void Iniciar()
        {
            Timer t = new Timer();
            t.Interval = 10;
            t.Tick += T_Tick;
            t.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            RenderizarPainel();
            ControlarCambio();
        }

        private void RenderizarPainel()
        {
            mem_rpm = mem.Pointer(true, 0x012E249C, 0xD8, 0x8, 0x0, 0x1C,0x4,0x10,0x14c);
            mem_temp_oleo = mem.Pointer(true, 0x0133076C, 0x24, 0x18, 0x248, 0x708);
            mem_temp_motor = mem.Pointer(true, 0x0133076C, 0x24, 0x18, 0x248, 0x708+0x8);
            mem_hodometro = mem.Pointer(true, 0x01330778, 0x10, 0x48, 0xc8 - 0xac);
            mem_combustivel = mem.Pointer(true, 0x01330778, 0x10, 0x48, 0xc8);
            mem_velocidade = mem.Pointer(true, 0x01330750, 0xfa8);
            mem_cambio = mem.Pointer(true, 0x01330778, 0x10, 0x48, 0x10, 0x14, 0x20, 0xc4, 0x0);
           




            painel_gasolina.Value = mem.ReadFloat(mem_combustivel) * 10;
            painel_rpm.Value = mem.ReadFloat(mem_rpm);
            painel_velocimetro.Value = mem.ReadFloat(mem_velocidade);
            painel_velocidade.Content = Math.Round(mem.ReadFloat(mem_velocidade), 0);
            painel_temp.Value = mem.ReadFloat(mem_temp_motor);
            painel_tempOleo.Value = mem.ReadFloat(mem_temp_oleo);
            painel_hodometro.Content = mem.ReadInt(mem_hodometro);
            painel_cambiocvt.Value = cambio;
            cambio = mem.ReadFloat(mem_cambio);
            rpm_corrente = mem.ReadFloat(mem_rpm);
        }

        private void ControlarCambio()
        {
           
            rpm_divergente = rpm_alvo - rpm_corrente;

            if (Math.Abs(rpm_divergente) < 1) indice_max = 0f;
            if (Math.Abs(rpm_divergente) > 2) indice_max = 0.0001f;
            if (Math.Abs(rpm_divergente) > 3) indice_max = 0.0005f;
            if (Math.Abs(rpm_divergente) > 5) indice_max = 0.005f;
            if (Math.Abs(rpm_divergente) > 10) indice_max = 0.01f;
            if (Math.Abs(rpm_divergente) > 20) indice_max = 0.02f;
            if (Math.Abs(rpm_divergente) > 30) indice_max = 0.05f;
            if (Math.Abs(rpm_divergente) > 40) indice_max = 0.1f;
            if (Math.Abs(rpm_divergente) > 50) indice_max = 0.20f;
            if (Math.Abs(rpm_divergente) > 100) indice_max = 0.25f;
            if (Math.Abs(rpm_divergente) > 200) indice_max = 0.5f;
            if (Math.Abs(rpm_divergente) > 300) indice_max = 0.5f;
            if (Math.Abs(rpm_divergente) > 400) indice_max = 2f;
            if (Math.Abs(rpm_divergente) > 500) indice_max = 6f;
            if (Math.Abs(rpm_divergente) > 600) indice_max = 7f;
            if (Math.Abs(rpm_divergente) > 700) indice_max = 8f;
            if (Math.Abs(rpm_divergente) > 800) indice_max = 10f;
            if (Math.Abs(rpm_divergente) > 900) indice_max = 15f;


            rpm_divergente = rpm_divergente * ajuste_deDivergencia;
            indice_ajuste = (rpm_corrente * indice_max) / rpm_divergente;

            if (indice_ajuste > 0) indice_ajuste = indice_max;
            if (indice_ajuste < 0) indice_ajuste = -indice_max;


            painel_cambio.Value = indice_ajuste;
            
            mem.WriteFloat(mem_cambio, cambio + indice_ajuste);

            cambio = mem.ReadFloat(mem_cambio);
            if (cambio >= max_cambio)
                mem.WriteFloat(mem_cambio, max_cambio);

            if (cambio <= 0.00001F)
                mem.WriteFloat(mem_cambio, 0.011F);

        }
    }
}
