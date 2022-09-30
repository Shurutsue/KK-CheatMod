namespace Cheat_Mod
{
    internal struct ReagentValues
    {
        public float Volume { get; set; }
        public string Name { get; set; }
        public short ID { get; set; }
        public ReagentValues(Reagent reagent)
        {
            this.Volume = reagent.volume;
            this.ID = reagent.id;
            this.Name = ReagentDatabase.GetReagent(ID).name;
        }
        public ReagentValues(float volume, string name, short id)
        {
            this.Volume = volume;
            this.Name = name;
            this.ID = id;
        }
    }
}
