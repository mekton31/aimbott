using Aimbot;
using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;

// init swed
Swed swed = new Swed("cs2");

//a lot of adres will be relative to client
IntPtr client = swed.GetModuleBase("client.dll"); 


//init imgui and over lay
Renderer renderer = new Renderer();
renderer.Start().Wait();

// Entity handling
List<Entity> entities = new List<Entity>(); // all ents
Entity localPlayer = new Entity(); //only our charhecter

//consts
const int HOTKEY = 0xA4; // hotkey

//aimbot loop
while (true)
{
    entities.Clear();

    //get entity list
    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);

    //first entry
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    //update lcoalplayer inf
    localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
    localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);
    localPlayer.view = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vecViewOffset);

    //loop for through entity list
    for (int i = 0; i < 64; i++) // 64 contrallers
    {
        if (listEntry == IntPtr.Zero) // jsut skip if entry is invaild
            continue;

        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78); //step 0x78

        if (currentController == IntPtr.Zero) // same idea
            continue;
        // get pawn
        int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);

        if (pawnHandle == 0) // obv?
            continue;

        // second entry, now we are gonna get specific pawn

        //apply bitmask 0x7FFF and shift  bits by9
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        // get pawn w 1ff mask
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

        if (currentPawn == localPlayer.pawnAddress) // if the enity is us
            continue;


        // get pawn attributes
        int health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);

        //if attribures hold up we add to our own entity list
        if (lifeState != 256)
            continue;
        if (health == localPlayer.team && !renderer.aimOnTeam)
            continue;

        Entity entity = new Entity();

        entity.pawnAddress = currentPawn;
        entity.controllerAddress = currentController;
        entity.health = health;
         //entity.team = team; 
        entity.lifeState = lifeState;
        entity.origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
        entity.view = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset);
        entity.distance = Vector3.Distance(entity.origin, localPlayer.origin);

        entities.Add(entity);

        //draw to console

    }
    //sort entities and aim
    entities = entities.OrderBy(o => o.distance).ToList(); //closest

    if (entities.Count > 0 && GetAsyncKeyState(HOTKEY) <0 && renderer.aimbot) //hotkey checkbox
    {
        //get view pos
        Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view);
        Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);

        //get angles
        Vector2 newAngles = Calculate.CalculateAngles(playerView, entityView);
        Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f);


        //force enw angles
        swed.WriteVec(client,Offsets.dwViewAngles, newAnglesVec3);
    }
    Thread.Sleep(20); //let cpu

}
// hotkey import
[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);