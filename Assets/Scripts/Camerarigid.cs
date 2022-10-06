using UnityEngine;

public class Camerarigid : MonoBehaviour
{
    public Transform player;
    public LayerMask layer;
    private void FixedUpdate(){
        RaycastHit hit;
        var dist = player.position - transform.position;
        
        if(Physics.Raycast(transform.position,player.position,out hit, dist.magnitude,layer)){
            string name = hit.collider.gameObject.tag;
            if (name != "MainCamera")
                {
            Debug.Log(1);
                float currentDistance = Vector3.Distance(hit.point,player.position);
                if (currentDistance < dist.magnitude)
                {
                    transform.position = hit.point;
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, player.position);
    }
}
