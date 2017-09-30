using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CloudController : MonoBehaviour
{

    GameObject cloud1;

    GameObject cloud2;

    GameObject cloud3;

    GameObject cloud4;

    // Use this for initialization
    void Start()
    {
        cloud1 = GameObject.Find("cloud1");
        cloud2 = GameObject.Find("cloud2");
        cloud3 = GameObject.Find("cloud3");
        cloud4 = GameObject.Find("cloud4");

        StartCoroutine(MoveCloud(cloud1, 0.01f));
        StartCoroutine(MoveCloud(cloud2, -0.02f));
        StartCoroutine(MoveCloud(cloud3, -0.04f));
        StartCoroutine(MoveCloud(cloud4, 0.02f));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    IEnumerator MoveCloud(GameObject cloud, float velocity)
    {
        while (true)
        {
            yield return new WaitForSeconds(.1f);
            var pos = cloud.transform.position;
            if (cloud.transform.position.x < -4f)
            {
                pos.x = 4f;
                cloud.transform.position = pos;
            }
            else
            {
                pos.x -= velocity;
                cloud.transform.position = pos;
            }
        }
    }
}
