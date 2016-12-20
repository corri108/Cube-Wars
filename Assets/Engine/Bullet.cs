using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour 
{
    [HideInInspector]
	public Vector3 velocity;
    [HideInInspector]
    public Vector3 startPos;
    [HideInInspector]
    public Vector3 endPos;
    [HideInInspector]
	public LineRenderer line;

    //non hidden
	public int type = 0;
	// Use this for initialization
	void Start () 
	{
		line = GetComponent<LineRenderer> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		startPos += velocity;
		endPos += velocity;

		line.SetPosition (0, startPos);
		line.SetPosition (1, endPos);
	}

	public void Set(Vector3 start, Vector3 end, Vector3 velocity)
	{
		line = GetComponent<LineRenderer> ();
		this.velocity = velocity;
		if(type == 0)
		{
			line.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
			line.SetColors(Color.white, Color.yellow);
			end += velocity * 2f;
			line.SetWidth(0.05f, 0.05f);
			line.SetPosition(0, start);
			line.SetPosition(1, end);
		}
		else if(type == 1)
		{
			line.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
			line.SetColors(Color.blue, Color.cyan);
			end += velocity * 4f;
			line.SetWidth(0.1f, 0.1f);
			line.SetPosition(0, start);
			line.SetPosition(1, end);
		}
        else if(type == 2)
        {
            line.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            line.SetColors(Color.red, Color.yellow);
            end += velocity * 4f;
            line.SetWidth(0.1f, 0.1f);
            line.SetPosition(0, start);
            line.SetPosition(1, end);

            for(int i = 0; i < 8; ++i)
            {
                GameObject bul = (GameObject)GameObject.Instantiate(this.gameObject, this.transform.position, this.transform.rotation);
                Bullet b = bul.GetComponent<Bullet>();
                b.line.material = line.material;
                b.line.SetColors(Color.red, Color.yellow);
                b.line.SetWidth(.1f, .1f);
                b.line.SetPosition(0, start);
                b.line.SetPosition(1, end + this.transform.right * Random.Range(-1f, 1f) + this.transform.up * Random.Range(-1f, 1f));
            }
        }

		startPos = start;
		endPos = end;
	}

	void OnCollisionEnter(Collision c)
	{
		if(c.gameObject.tag.Equals("Tree"))
		{
			GameObject.Destroy(c.gameObject.transform.root.gameObject);
		}
	}

	void DrawLine(Vector3 start, Vector3 end, Color color, float duration = .1f)
	{
		/*
		GameObject myLine = new GameObject();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors(color, color);
		lr.SetWidth(0.1f, 0.1f);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		GameObject.Destroy(myLine, duration);*/
	}
}
