using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cheat_Mod
{
    internal class StringKoboldGenes
    {
        public string maxEnergy = "";
        public string baseSize = "";
        public string fatSize = "";
        public string ballSize = "";
        public string dickSize = "";
        public string breastSize = "";
        public string bellySize = "";
        public string metabCap = "";
        public string hue = "";
        public string brightness = "";
        public string dickEquip = "";
        public string dickThickness = "";
        public string grabCount = "";

        public StringKoboldGenes() { }
        public StringKoboldGenes(KoboldGenes kg)
        {
            this.maxEnergy = Convert.ToString(kg.maxEnergy);
            this.baseSize = Convert.ToString(kg.baseSize);
            this.fatSize = Convert.ToString(kg.fatSize);
            this.ballSize = Convert.ToString(kg.ballSize);
            this.dickSize = Convert.ToString(kg.dickSize);
            this.breastSize = Convert.ToString(kg.breastSize);
            this.bellySize = Convert.ToString(kg.bellySize);
            this.metabCap = Convert.ToString(kg.metabolizeCapacitySize);
            this.hue = Convert.ToString(kg.hue);
            this.brightness = Convert.ToString(kg.brightness);
            this.dickEquip = Convert.ToString(kg.dickEquip);
            this.dickThickness = Convert.ToString(kg.dickThickness);
            this.grabCount = Convert.ToString(kg.grabCount);
        }

        public void SetGenes(Kobold k)
        {
            KoboldGenes kg = k.GetGenes();
            kg.maxEnergy = Convert.ToByte(this.maxEnergy);
            kg.baseSize = Convert.ToSingle(this.baseSize);
            kg.fatSize = Convert.ToSingle(this.fatSize);
            kg.ballSize = Convert.ToSingle(this.ballSize);
            kg.dickSize = Convert.ToSingle(this.dickSize);
            kg.breastSize = Convert.ToSingle(this.breastSize);
            kg.bellySize = Convert.ToSingle(this.bellySize);
            kg.metabolizeCapacitySize = Convert.ToSingle(this.metabCap);
            kg.hue = Convert.ToByte(this.hue);
            kg.brightness = Convert.ToByte(this.brightness);
            kg.dickEquip = Convert.ToByte(this.dickEquip);
            kg.dickThickness = Convert.ToSingle(this.dickThickness);
            kg.grabCount = Convert.ToByte(this.grabCount);
            k.SetGenes(kg);
        }
    }
}
