using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class MapLoader : MonoBehaviour {

    [XmlType("map")]
    public class Map {

        public Map()
        {
            objects = new List<Object>();
            terrains = new List<Terrain>();
        }

        [XmlArray("map_terrains")]
        [XmlArrayItem("map_terrain")]
        public List<Terrain> terrains { get; set; }

        [XmlArray("map_objects")]
        [XmlArrayItem("map_object")]
        public List<Object> objects { get; set; }
    }

    [XmlType("object")]
    public class Object {
        [XmlElement("object_pos")]
        public Vector3 position;
        [XmlElement("object_rot")]
        public Quaternion rotation;
        [XmlElement("object_scale")]
        public Vector3 scale;
        [XmlElement("object_type_id")]
        public int typeid;

        public override string ToString()
        {
            return typeid.ToString() + " | Pos: " + position.ToString() + " Rot: " + rotation.ToString() + " Scale: " + scale.ToString();
        }
    }

    [XmlType("terrain")]
    public class Terrain {

        public Terrain()
        {
            heightmap = new List<string>();
            //WIP
            width = 500;
            length = 500;
            height = 600;
        }

        [XmlElement("terrain_width")]
        public int width;
        [XmlElement("terrain_length")]
        public int length;
        [XmlElement("terrain_height")]
        public int height;
        [XmlElement("terrain_res")]
        public int res;
        [XmlElement("terrain_detail_res")]
        public int detres;
        [XmlElement("terrain_detail_res_per")]
        public int detres_per;
        [XmlElement("terrain_texture_res")]
        public int tex_res;
        [XmlElement("terrain_base_tex_res")]
        public int basetex_res;


        [XmlElement("terrain_objectdata")]
        public Object obj_data;

        //[XmlElement("terrain_objectdata")]
        //public string heightmap;

        [XmlArray("terrain_heights")]
        [XmlArrayItem("terrain_height")]
        public List<string> heightmap;
    }

    [XmlType("height")]
    public class Height {
        [XmlElement("height_x")]
        public int x;
        [XmlElement("height_y")]
        public int y;
        [XmlElement("height_height")]
        public float height;
    }

    public bool load;
    public GameObject terrain;

	// Use this for initialization
	void Start () {
        if (load)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            LoadMap(Application.dataPath + "/map");
        }
        else
            SaveMap(Application.dataPath + "/map");
    }

    // Update is called once per frame
    void Update () {
		
	}

    void LoadMap(string mapfile)
    {
        var serializer = new XmlSerializer(typeof(Map));
        var reader = XmlReader.Create(mapfile + ".xml");
        var result = (Map)serializer.Deserialize(reader);
        reader.Close();
        foreach (Object obj in result.objects)
        {
            var obj2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj2.name = obj.typeid.ToString();
            obj2.transform.position = obj.position;
            obj2.transform.rotation = obj.rotation;
            obj2.transform.localScale = obj.scale;
            obj2.transform.parent = this.transform;
            Debug.Log(obj.ToString());
        }
        foreach (Terrain obj in result.terrains)
        {
            var obj2 = GameObject.Instantiate(terrain);
            obj2.name = obj.obj_data.typeid.ToString();
            obj2.transform.position = obj.obj_data.position;
            obj2.transform.rotation = obj.obj_data.rotation;
            obj2.transform.localScale = obj.obj_data.scale;
            obj2.transform.parent = this.transform;
            obj2.GetComponent<UnityEngine.Terrain>().terrainData = new TerrainData();
            obj2.GetComponent<UnityEngine.Terrain>().terrainData.heightmapResolution = obj.res;
            obj2.GetComponent<UnityEngine.Terrain>().terrainData.size = new Vector3(obj.width, obj.height, obj.length);
            foreach (string height in obj.heightmap)
            {
                var tex = new Texture2D(obj.width, obj.length);
                WWW www = new WWW("file://" + mapfile + height + ".png");
                www.LoadImageIntoTexture(tex);
                //var bin = new BinaryReader(new StreamReader(mapfile + height + ".png").BaseStream);
                //var newbin = bin.Read((int)bin.BaseStream.Length - 1);
                //ImageConversion.LoadImage(tex, newbin);
                Color[] cols = tex.GetPixels(0, 0, tex.width, tex.height);
                //float[,] heights = new float[,] { };
                float[,] heights = new float[tex.width, tex.height];
                for (int x = 0; x < tex.width; x++)
                {
                    for (int y = 0; y < tex.height; y++)
                    {
                        heights[x, y] = cols[y * tex.width + x].grayscale;
                        //tex.GetPixel(x, y).grayscale;
                        //Debug.Log(tex.GetPixel(x, y).grayscale);
                        //ter.heightmap.Add(hgt);
                    }
                }
                obj2.GetComponent<UnityEngine.Terrain>().terrainData.SetHeights(0, 0, heights);
                //float[,] stuff = { { height.height } };
                //obj2.GetComponent<UnityEngine.Terrain>().terrainData.SetHeights(height.x, height.y, new float[1, 1] { { height.height } });
            }
            //Debug.Log(obj.ToString());
        }
    }

    void SaveMap(string mapfile)
    {
        var map = new Map();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<UnityEngine.Terrain>() != null)
            {
                int xs = (int)transform.GetChild(i).GetComponent<UnityEngine.Terrain>().terrainData.size.x;
                int ys = (int)transform.GetChild(i).GetComponent<UnityEngine.Terrain>().terrainData.size.z;
                int zh = (int)transform.GetChild(i).GetComponent<UnityEngine.Terrain>().terrainData.size.y;
                var ter = new Terrain();
                ter.width = xs;
                ter.length = ys;
                ter.height = zh;
                ter.res = transform.GetChild(i).GetComponent<UnityEngine.Terrain>().terrainData.heightmapResolution;
                var tex = new Texture2D(xs, ys);
                for (int x = 0; x < xs; x++)
                {
                    for (int y = 0; y < ys; y++)
                    {
                        var hgt = new Height();
                        hgt.height = transform.GetChild(i).GetComponent<UnityEngine.Terrain>().terrainData.GetHeight(x, y) / transform.GetChild(i).GetComponent<UnityEngine.Terrain>().terrainData.size.y;
                        hgt.x = x;
                        hgt.y = y;
                        var c = new Color(hgt.height, hgt.height, hgt.height, 255);
                        //swap cuz reason
                        tex.SetPixel(y, x, c);
                        //ter.heightmap.Add(hgt);
                    }
                }
                tex.Apply();
                var bin = new BinaryWriter(new StreamWriter(mapfile + i.ToString() + ".png").BaseStream);
                ter.heightmap.Add(i.ToString());
                bin.Write(tex.EncodeToPNG());
                bin.Close();
                ter.obj_data = new Object();
                ter.obj_data.typeid = i;
                ter.obj_data.position = transform.GetChild(i).position;
                ter.obj_data.rotation = transform.GetChild(i).rotation;
                ter.obj_data.scale = transform.GetChild(i).localScale;
                map.terrains.Add(ter);
            }
            else
            {
                var obj = new Object();
                obj.typeid = i;
                obj.position = transform.GetChild(i).position;
                obj.rotation = transform.GetChild(i).rotation;
                obj.scale = transform.GetChild(i).localScale;
                map.objects.Add(obj);
            }
        }
        foreach (Object obj in map.objects)
        {
            Debug.Log(obj.ToString());
        }
        var serializer = new XmlSerializer(typeof(Map));
        var writer = XmlWriter.Create(mapfile + ".xml");
        serializer.Serialize(writer, map);
        writer.Close();
    }
}
