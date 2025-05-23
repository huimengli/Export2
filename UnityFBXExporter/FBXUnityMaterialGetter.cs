﻿// ===============================================================================================
//	The MIT License (MIT) for UnityFBXExporter
//
//  UnityFBXExporter was created for Building Crafter (http://u3d.as/ovC) a tool to rapidly 
//	create high quality buildings right in Unity with no need to use 3D modeling programs.
//
//  Copyright (c) 2016 | 8Bit Goose Games, Inc.
//		
//	Permission is hereby granted, free of charge, to any person obtaining a copy 
//	of this software and associated documentation files (the "Software"), to deal 
//	in the Software without restriction, including without limitation the rights 
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
//	of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//		
//	The above copyright notice and this permission notice shall be included in all 
//	copies or substantial portions of the Software.
//		
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//	PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//	HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//	OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
//	OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ===============================================================================================

using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using FE = UnityFBXExporter.FBXExporter;

namespace UnityFBXExporter
{
    /// <summary>
    /// 
    /// </summary>
	public class FBXUnityMaterialGetter
	{

        /// <summary>
        /// Finds all materials in a gameobject and writes them to a string that can be read by the FBX writer
        /// </summary>
        /// <param name="gameObj">Parent GameObject being exported.</param>
        /// <param name="newPath">The path to export to.</param>
        /// <param name="materials">Materials which were written to this fbx file.</param>
        /// <param name="matObjects">The material objects to write to the file.</param>
        /// <param name="connections">The connections to write to the file.</param>
        /// <param name="copyMaterials">The connections to write to the file.</param>
        /// <param name="copyTextures"></param>
        public static void GetAllMaterialsToString(GameObject gameObj, string newPath, bool copyMaterials, bool copyTextures, out Material[] materials, out string matObjects, out string connections)
		{
			StringBuilder tempObjectSb = new StringBuilder();
			StringBuilder tempConnectionsSb = new StringBuilder();

            // Need to get all unique materials for the submesh here and then write them in
            //@cartzhang modify.As meshrender and skinnedrender is same level in inherit relation shape.
            // if not check,skinned render ,may lost some materials.
            Renderer[] meshRenders = gameObj.GetComponentsInChildren<Renderer>();
			
			List<Material> uniqueMaterials = new List<Material>();

			// Gets all the unique materials within this GameObject Hierarchy
			for(int i = 0; i < meshRenders.Length; i++)
			{
				for(int n = 0; n < meshRenders[i].sharedMaterials.Length; n++)
				{
					Material mat = meshRenders[i].sharedMaterials[n];
					
					if(uniqueMaterials.Contains(mat) == false && mat != null)
					{
						uniqueMaterials.Add(mat);
					}
				}
			}

            for (int i = 0; i < uniqueMaterials.Count; i++)
			{
				Material mat = uniqueMaterials[i];

				// We rename the material if it is being copied
				string materialName = mat.name;
				if(copyMaterials)
					materialName = gameObj.name + "_" + mat.name;

				int referenceId = Mathf.Abs(mat.GetInstanceID());

				tempObjectSb.AppendLine();
				tempObjectSb.AppendLine("\tMaterial: " + referenceId + ", \"Material::" + materialName + "\", \"\" {");
				tempObjectSb.AppendLine("\t\tVersion: 102");
				tempObjectSb.AppendLine("\t\tShadingModel: \"phong\"");
				tempObjectSb.AppendLine("\t\tMultiLayer: 0");
				tempObjectSb.AppendLine("\t\tProperties70:  {");
				tempObjectSb.AppendFormat("\t\t\tP: \"Diffuse\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", FE.FBXFormat(mat.color.r), FE.FBXFormat(mat.color.g), FE.FBXFormat(mat.color.b));
				tempObjectSb.AppendLine();
				tempObjectSb.AppendFormat("\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",{0},{1},{2}", FE.FBXFormat(mat.color.r), FE.FBXFormat(mat.color.g), FE.FBXFormat(mat.color.b));
				tempObjectSb.AppendLine();

				// TODO: Figure out if this property can be written to the FBX file
	//			if(mat.HasProperty("_MetallicGlossMap"))
	//			{
	//				Debug.Log("has metallic gloss map");
	//				Color color = mat.GetColor("_Color");
	//				tempObjectSb.AppendFormat("\t\t\tP: \"Specular\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", color.r, color.g, color.r);
	//				tempObjectSb.AppendLine();
	//				tempObjectSb.AppendFormat("\t\t\tP: \"SpecularColor\", \"ColorRGB\", \"Color\", \" \",{0},{1},{2}", color.r, color.g, color.b);
	//				tempObjectSb.AppendLine();
	//			}

				if(mat.HasProperty("_SpecColor"))
				{
					Color color = mat.GetColor("_SpecColor");
					tempObjectSb.AppendFormat("\t\t\tP: \"Specular\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.r));
					tempObjectSb.AppendLine();
					tempObjectSb.AppendFormat("\t\t\tP: \"SpecularColor\", \"ColorRGB\", \"Color\", \" \",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.b));
					tempObjectSb.AppendLine();
				}

				if(mat.HasProperty("_Mode"))
				{
					Color color = Color.white;

					switch((int)mat.GetFloat("_Mode"))
					{
					case 0: // Map is opaque

						break;

					case 1: // Map is a cutout
						//  TODO: Add option if it is a cutout
						break;

					case 2: // Map is a fade
						color = mat.GetColor("_Color");

							tempObjectSb.AppendFormat("\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.b));
							tempObjectSb.AppendLine();
							tempObjectSb.AppendFormat("\t\t\tP: \"Opacity\", \"double\", \"Number\", \"\",{0}", FE.FBXFormat(color.a));
							tempObjectSb.AppendLine();
						break;

					case 3: // Map is transparent
						color = mat.GetColor("_Color");

							tempObjectSb.AppendFormat("\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.b));
							tempObjectSb.AppendLine();
							tempObjectSb.AppendFormat("\t\t\tP: \"Opacity\", \"double\", \"Number\", \"\",{0}", FE.FBXFormat(color.a));
							tempObjectSb.AppendLine();
						break;
					}
				}

				// NOTE: Unity doesn't currently import this information (I think) from an FBX file.
				if(mat.HasProperty("_EmissionColor"))
				{
					Color color = mat.GetColor("_EmissionColor");

					tempObjectSb.AppendFormat("\t\t\tP: \"Emissive\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.b));
					tempObjectSb.AppendLine();

					float averageColor = (color.r + color.g + color.b) / 3f;

					tempObjectSb.AppendFormat("\t\t\tP: \"EmissiveFactor\", \"Number\", \"\", \"A\",{0}", FE.FBXFormat(averageColor));
					tempObjectSb.AppendLine();
				}

				// TODO: Add these to the file based on their relation to the PBR files
//				tempObjectSb.AppendLine("\t\t\tP: \"AmbientColor\", \"Color\", \"\", \"A\",0,0,0");
//				tempObjectSb.AppendLine("\t\t\tP: \"ShininessExponent\", \"Number\", \"\", \"A\",6.31179285049438");
//				tempObjectSb.AppendLine("\t\t\tP: \"Ambient\", \"Vector3D\", \"Vector\", \"\",0,0,0");
//				tempObjectSb.AppendLine("\t\t\tP: \"Shininess\", \"double\", \"Number\", \"\",6.31179285049438");
//				tempObjectSb.AppendLine("\t\t\tP: \"Reflectivity\", \"double\", \"Number\", \"\",0");

				tempObjectSb.AppendLine("\t\t}");
				tempObjectSb.AppendLine("\t}");

				string textureObjects;
				string textureConnections;

				SerializedTextures(gameObj, newPath, mat, materialName, copyTextures, out textureObjects, out textureConnections);

				tempObjectSb.Append(textureObjects);
				tempConnectionsSb.Append(textureConnections);
			}

			materials = uniqueMaterials.ToArray<Material>();

			matObjects = tempObjectSb.ToString();
			connections = tempConnectionsSb.ToString();
		}

        /// <summary>
        /// Serializes textures to FBX format.
        /// </summary>
        /// <param name="gameObj">Parent GameObject being exported.</param>
        /// <param name="newPath">The path to export to.</param>
        /// <param name="material">Materials that holds all the textures.</param>
        /// <param name="connections">The string to connect this to the  material.</param>
        /// <param name="materialName"></param>
        /// <param name="copyTextures"></param>
        /// <param name="objects"></param>
        private static void SerializedTextures(GameObject gameObj, string newPath, Material material, string materialName, bool copyTextures, out string objects, out string connections)
		{
			// TODO: FBX import currently only supports Diffuse Color and Normal Map
			// Because it is undocumented, there is no way to easily find out what other textures
			// can be attached to an FBX file so it is imported into the PBR shaders at the same time.
			// Also NOTE, Unity 5.1.2 will import FBX files with legacy shaders. This is fix done
			// in at least 5.3.4.

			StringBuilder objectsSb = new StringBuilder();
			StringBuilder connectionsSb = new StringBuilder();

			int materialId = Mathf.Abs(material.GetInstanceID());

			Texture mainTexture = material.GetTexture("_MainTex");

			string newObjects = null;
			string newConnections = null;

			// Serializeds the Main Texture, one of two textures that can be stored in FBX's sysytem
			if(mainTexture != null)
			{
				SerializeOneTexture(gameObj, newPath, material, materialName, materialId, copyTextures, "_MainTex", "DiffuseColor", out newObjects, out newConnections);
				objectsSb.AppendLine(newObjects);
				connectionsSb.AppendLine(newConnections);
			}

			if(SerializeOneTexture(gameObj, newPath, material, materialName, materialId, copyTextures, "_BumpMap", "NormalMap", out newObjects, out newConnections))
			{
				objectsSb.AppendLine(newObjects);
				connectionsSb.AppendLine(newConnections);
			}

			connections = connectionsSb.ToString();
			objects = objectsSb.ToString();
		}

		private static bool SerializeOneTexture(GameObject gameObj, 
		                                        string newPath, 
		                                        Material material, 
		                                        string materialName,
		                                        int materialId,
		                                        bool copyTextures, 
		                                        string unityExtension, 
		                                        string textureType, 
		                                        out string objects, 
		                                        out string connections)
		{
			StringBuilder objectsSb = new StringBuilder();
			StringBuilder connectionsSb = new StringBuilder();

			Texture texture = material.GetTexture(unityExtension);

			if(texture == null)
			{
				objects = "";
				connections = "";
				return false;
			}
			string originalAssetPath = "";

#if UNITY_EDITOR
			originalAssetPath = AssetDatabase.GetAssetPath(texture);
#else
			Debug.LogError("Unity FBX Exporter can not serialize textures at runtime (yet). Look in FBXUnityMaterialGetter around line 250ish. Fix it and contribute to the project!");
			objects = "";
			connections = "";
			return false;
#endif
			string fullDataFolderPath = Application.dataPath;
			string textureFilePathFullName = originalAssetPath;
			string textureName = Path.GetFileNameWithoutExtension(originalAssetPath);
			string textureExtension = Path.GetExtension(originalAssetPath);

			// If we are copying the textures over, we update the relative positions
			if(copyTextures)
			{
				int indexOfAssetsFolder = fullDataFolderPath.LastIndexOf("/Assets");
				fullDataFolderPath = fullDataFolderPath.Remove(indexOfAssetsFolder, fullDataFolderPath.Length - indexOfAssetsFolder);
				
				string newPathFolder = newPath.Remove(newPath.LastIndexOf('/') + 1, newPath.Length - newPath.LastIndexOf('/') - 1);
				textureName = gameObj.name + "_" + material.name + unityExtension;

				textureFilePathFullName = fullDataFolderPath + "/" + newPathFolder + textureName + textureExtension;
			}

			long textureReference = FBXExporter.GetRandomFBXId();

			// TODO - test out different reference names to get one that doesn't load a _MainTex when importing.

			objectsSb.AppendLine("\tTexture: " + textureReference + ", \"Texture::" + materialName + "\", \"\" {");
			objectsSb.AppendLine("\t\tType: \"TextureVideoClip\"");
			objectsSb.AppendLine("\t\tVersion: 202");
			objectsSb.AppendLine("\t\tTextureName: \"Texture::" + materialName + "\"");
			objectsSb.AppendLine("\t\tProperties70:  {");
			objectsSb.AppendLine("\t\t\tP: \"CurrentTextureBlendMode\", \"enum\", \"\", \"\",0");
			objectsSb.AppendLine("\t\t\tP: \"UVSet\", \"KString\", \"\", \"\", \"map1\"");
			objectsSb.AppendLine("\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",1");
			objectsSb.AppendLine("\t\t}");
			objectsSb.AppendLine("\t\tMedia: \"Video::" + materialName + "\"");

			// Sets the absolute path for the copied texture
			objectsSb.Append("\t\tFileName: \"");
			objectsSb.Append(textureFilePathFullName);
			objectsSb.AppendLine("\"");
			
			// Sets the relative path for the copied texture
			// TODO: If we don't copy the textures to a relative path, we must find a relative path to write down here
			if(copyTextures)
				objectsSb.AppendLine("\t\tRelativeFilename: \"/Textures/" + textureName + textureExtension + "\"");

			objectsSb.AppendLine("\t\tModelUVTranslation: 0,0"); // TODO: Figure out how to get the UV translation into here
			objectsSb.AppendLine("\t\tModelUVScaling: 1,1"); // TODO: Figure out how to get the UV scaling into here
			objectsSb.AppendLine("\t\tTexture_Alpha_Source: \"None\""); // TODO: Add alpha source here if the file is a cutout.
			objectsSb.AppendLine("\t\tCropping: 0,0,0,0");
			objectsSb.AppendLine("\t}");
			
			connectionsSb.AppendLine("\t;Texture::" + textureName + ", Material::" + materialName + "\"");
			connectionsSb.AppendLine("\tC: \"OP\"," + textureReference + "," + materialId + ", \"" + textureType + "\""); 
			
			connectionsSb.AppendLine();

			objects = objectsSb.ToString();
			connections = connectionsSb.ToString();

			return true;
		}
	}
}