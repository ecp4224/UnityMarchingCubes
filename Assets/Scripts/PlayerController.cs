using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : BindableMonoBehavior
{
    [BindComponent(fromObject = "Voxel World", warnOnly = true)]
    private World world;

    public LayerMask mask;
    public float clickDistance = 3f;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        world.players.Add(gameObject);
    }
    
    void Update()
    {
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 6.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 6.0f;

        transform.Translate(x, 0, z);


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, clickDistance, mask))
            {
                Chunk c = world.ChunkAt(hit.point);
                if (c != null)
                {
                    Debug.Log("Hit inside " + hit.point + " which is in chunk " + c.Position);
                    
                    float value = c.GetSample(hit.point);
                    
                    Debug.Log("Got point value of " + value + " in chunk " + c.Position);
                    
                    if (Input.GetKeyDown(KeyCode.Mouse1))
                        world.DeletePoint(hit.point);
                    else
                        world.AddPoint(hit.point);
                }
            }
        }
    }
}