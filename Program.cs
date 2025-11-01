namespace WinFormsApp1
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // << الحل هنا >>: نشغل الكلاس بالاسم الجديد
            Application.Run(new JsonTranslator());
        }
    }
}