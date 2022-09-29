namespace Cheat_Mod
{
    internal struct ReagentValues
    {
        public float volume { get; set; }
        public string name { get; set; }
        public short id { get; set; }
        public ReagentValues(Reagent reagent)
        {
            this.volume = reagent.volume;
            this.id = reagent.id;
            this.name = ReagentDatabase.GetReagent(id).name;
        }
        public ReagentValues(float volume, string name, short id)
        {
            this.volume = volume;
            this.name = name;
            this.id = id;
        }
    }
}
