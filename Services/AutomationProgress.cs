namespace Job_Finder.Services
{
    public class AutomationProgress
    {
        private int progress {  get; set; }
        public async Task<int> GetProgress()
        {
            return progress;
        }
        public async Task SetProgress(int progress) 
        {
            this.progress = progress; 
        }
    }
}
