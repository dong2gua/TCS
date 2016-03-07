namespace ComponentDataService.Types
{  
    public struct PhantomDefine
    {
        #region Enums
        public enum PhantomPattern { Lattice, Random }
        #endregion
        public double Radius { get; set; }
        public double Distance { get; set; }
        public int Count { get; set; }
        public PhantomPattern Pattern { get; set; }
    }
}
