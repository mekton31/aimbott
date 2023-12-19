using ClickableTransparentOverlay;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aimbot
{
    public class Renderer : Overlay
    {
        public bool aimbot = false;
        public bool aimOnTeam = false;


        protected override void Render()
        {
            ImGui.Begin("test");

            ImGui.Checkbox("Aimbot", ref  aimbot);
            ImGui.Checkbox("Aimbot Team Players", ref aimOnTeam);
        }
    }
}
