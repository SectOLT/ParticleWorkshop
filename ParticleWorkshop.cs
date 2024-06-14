using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using ResoniteModLoader;

namespace ParticleWorkshop
{
    public class ParticleWorkshop : ResoniteMod
    {
        public override string Author => "Gareth48, Cyro, badhaloninja, TheJebForge";
        public override string Name => "ParticleWorkshop";
        public override string Version => "1.1.3";
        public override string Link => "https://github.com/TheJebForge/ParticleWorkshop/";
        
        static void ParticleAction(Slot s)
        {
            s.AttachComponent<DestroyOnUserLeave>().TargetUser.Target = Engine.Current.WorldManager.FocusedWorld.LocalUser;
            s.AttachComponent<DynamicVariableSpace>().SpaceName.Value = "ParticleEditor";
            DynamicReferenceVariable<ParticleStyle> particleStyleHolder = s.AttachComponent<DynamicReferenceVariable<ParticleStyle>>();
            particleStyleHolder.VariableName.Value = "ParticleStyle";

            s.PersistentSelf = false;

            //Create new panel with basic layout
            s.LocalScale *= 0.0009f;
            UIBuilder ui = RadiantUI_Panel.SetupPanel(s, (LocaleString)"Particle Workshop (Mod)", new float2(800f, 1024f));

            // Set up layout
            ui.ScrollArea();
            ui.VerticalLayout(4f, 8f, childAlignment: Alignment.TopLeft);
            ui.FitContent(SizeFit.Disabled, SizeFit.MinSize);
            ui.Style.MinHeight = 32f;
            ui.Text("Particle Style");

            //build the UI for the particle style input
            SyncMemberEditorBuilder.Build(particleStyleHolder.Reference, null, particleStyleHolder.GetSyncMemberFieldInfo("Reference"), ui);
            
            Slot colorListSlot = ui.Empty("Color Keyframes List");
            ui.Text("Color Keyframes List");
            ValueGradientDriver<colorX> colorValueGradient = colorListSlot.AttachComponent<ValueGradientDriver<colorX>>();
            
            colorValueGradient.Points.Changed += (c) =>{
                if(!colorValueGradient.Enabled){
                    return;
                }
                SyncList<ValueGradientDriver<colorX>.Point> points = colorValueGradient.Points;
                if (points == null) return;

                //Unity particles support a maximum of 8 keyframes, as a result we can't let any more points get added or it just breaks the array
                //Not in like a dangerous or interesting way the particle just goes white. Really boring stuff.
                if(points.Count > 8){
                    colorValueGradient.RunSynchronously(() => points.RemoveAt(points.Count-1));
                    return;
                }

                SyncLinear<colorX> colorList = particleStyleHolder.Reference.Target?.ColorOverLifetime;
                
                if(colorList == null) 
                    return;

                colorList.Clear();
                foreach (var t in points)
                {
                    colorList.InsertKey(t.Position, t.Value);
                }
            };

            particleStyleHolder.Reference.Changed += (c) => {
                ISyncRef r = c as ISyncRef;
                ParticleStyle target = r?.Target as ParticleStyle;
                
                //This just works -- prevents a weird writeback issue we had early on
                colorValueGradient.Enabled = false;
                colorValueGradient.Points.Clear();
                if (target == null)
                    return;

                target.UseColorOverLifetime.Value = true;

                SyncLinear<colorX> colorKeyframes = target.ColorOverLifetime;
                foreach (var key in colorKeyframes)
                {
                    colorValueGradient.AddPoint(key.time, key.value);
                }
                colorValueGradient.Enabled = true;
            };
            
            SyncMemberEditorBuilder.Build(colorValueGradient.Points, null, colorValueGradient.GetSyncMemberFieldInfo("Points"), ui);

            //go go gadget same code but with a float instead
            Slot alphaListSlot = ui.Empty("Alpha Keyframes List");
            ui.Text("Alpha Keyframes List");
            ValueGradientDriver<float> alphaValueGradient = alphaListSlot.AttachComponent<ValueGradientDriver<float>>();


            //Set up on change event
            alphaValueGradient.Points.Changed += (IChangeable c) =>{
                if(!alphaValueGradient.Enabled){
                    return;
                }
                SyncList<ValueGradientDriver<float>.Point> alphaPoints = alphaValueGradient.Points;
                if (alphaPoints == null) return;

                //Unity particles support a maximum of 8 keyframes, as a result we can't let any more points get added or it just breaks the array
                //Not in like a dangerous or interesting way the particle just goes white. Really boring stuff.
                if(alphaPoints.Count > 8){
                    alphaValueGradient.RunSynchronously(() => alphaPoints.RemoveAt(alphaPoints.Count-1));
                    return;
                }
                
                SyncLinear<float> alphaList = particleStyleHolder.Reference.Target?.AlphaOverLifetime;
                
                if(alphaList == null) 
                    return;

                alphaList.Clear();
                foreach (var t in alphaPoints)
                {
                    alphaList.InsertKey(t.Position, t.Value);
                }
            };

            particleStyleHolder.Reference.Changed += (c) => {
                ISyncRef r = c as ISyncRef;
                ParticleStyle target = r?.Target as ParticleStyle;
                
                alphaValueGradient.Enabled = false;
                alphaValueGradient.Points.Clear();

                if (target == null)
                    return;

                target.UseColorOverLifetime.Value = true;

                SyncLinear<float> alphaKeyframes = target.AlphaOverLifetime;
                foreach (var key in alphaKeyframes)
                {
                    alphaValueGradient.AddPoint(key.time, key.value);
                }
                alphaValueGradient.Enabled = true;
            };
            
            SyncMemberEditorBuilder.Build(alphaValueGradient.Points, null, alphaValueGradient.GetSyncMemberFieldInfo("Points"), ui);
        }

        public override void OnEngineInit()
        {
            //Harmony Patcher? I hardly even know her!
            Engine.Current.RunPostInit(() => {
                DevCreateNewForm.AddAction("Editor", "Particle Workshop Editor (Mod)", ParticleAction);
            });
        }
    }
}
