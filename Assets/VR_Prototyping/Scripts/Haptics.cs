using Valve.VR;

namespace VR_Prototyping.Scripts
{
    public static class Haptics
    {
       public static void Constant(SteamVR_Action_Vibration hapticAction, float frequency, float amplitude, SteamVR_Input_Sources source)
       {
            hapticAction.Execute(0, 5, frequency, amplitude, source);
       }
    }
}
