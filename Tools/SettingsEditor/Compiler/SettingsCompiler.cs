using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

using Sce.Atf;

namespace SettingsEditor
{
    public class SettingsCompiler
    {
		Assembly CompileSettings( string inputFilePath )
		{
			string fileName = Path.GetFileNameWithoutExtension( inputFilePath );

			string code = File.ReadAllText( inputFilePath );
			//code = "using SettingsCompiler;\r\n\r\n" + "namespace " + fileName + "\r\n{\r\n" + code;
			//code += "\r\n}";

			Dictionary<string, string> compilerOpts = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
			CSharpCodeProvider compiler = new CSharpCodeProvider( compilerOpts );

			string[] sources = { code };
			CompilerParameters compilerParams = new CompilerParameters();
			compilerParams.GenerateInMemory = true;
			compilerParams.ReferencedAssemblies.Add( "System.dll" );
			//compilerParams.ReferencedAssemblies.Add( "SettingsCompilerAttributes.dll" );
			compilerParams.ReferencedAssemblies.Add( "SettingsEditorAttributes.dll" );
			CompilerResults results = compiler.CompileAssemblyFromSource( compilerParams, sources );
			if ( results.Errors.HasErrors )
			{
				string errMsg = "Errors were returned from the C# compiler:\r\n\r\n";
				foreach ( CompilerError compilerError in results.Errors )
				{
					int lineNum = compilerError.Line - 4;
					errMsg += inputFilePath + "(" + lineNum + "): " + compilerError.ErrorText + "\r\n";
				}
				throw new Exception( errMsg );
			}

			return results.CompiledAssembly;
		}

		void ReflectType( Type settingsType, List<Type> enumTypes, SettingGroup group )
		{
			object settingsInstance = Activator.CreateInstance( settingsType );

			BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo[] fields = settingsType.GetFields( flags );
			foreach ( FieldInfo field in fields )
			{
				foreach ( Setting setting in group.Settings )
					if ( setting.LongName == field.Name )
						throw new Exception( string.Format( "Duplicate setting \"{0}\" detected", setting.LongName ) );

				Type fieldType = field.FieldType;
				object fieldValue = field.GetValue( settingsInstance );
				if ( fieldType == typeof( bool ) )
					group.Settings.Add( new BoolSetting( (bool) fieldValue, field, group ) );
				else if ( fieldType == typeof( int ) )
					group.Settings.Add( new IntSetting( (int) fieldValue, field, group ) );
				else if ( fieldType.IsEnum )
				{
					if ( enumTypes.Contains( fieldType ) == false )
						enumTypes.Add( fieldType );
					group.Settings.Add( new EnumSetting( fieldValue, field, fieldType, group ) );
				}
				else if ( fieldType == typeof( float ) )
					group.Settings.Add( new FloatSetting( (float) fieldValue, field, group ) );
				else if (fieldType == typeof( string ))
					group.Settings.Add( new StringSetting( (string) fieldValue, field, group ) );
				//else if ( fieldType == typeof( Direction ) )
				//	settings.Add( new DirectionSetting( (Direction) fieldValue, field, group ) );
				//else if ( fieldType == typeof( Orientation ) )
				//	settings.Add( new OrientationSetting( (Orientation) fieldValue, field, group ) );
				else if (fieldType == typeof( Color ))
					group.Settings.Add( new ColorSetting( (Color) fieldValue, field, group ) );
				else if (fieldType == typeof( Float4 ))
					group.Settings.Add( new Float4Setting( (Float4) fieldValue, field, group ) );
				else
					throw new Exception( "Invalid type for setting " + field.Name );
			}
		}

		void ReflectGeneratorConfig( Type settingsType )
		{
			object settingsInstance = Activator.CreateInstance( settingsType );

			BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo[] fields = settingsType.GetFields( flags );
			foreach (FieldInfo field in fields)
			{
				Type fieldType = field.FieldType;
				object fieldValue = field.GetValue( settingsInstance );
				if (fieldType == typeof( string ))
				{
					if (field.Name == "SettingsEditorHeaderInclude")
					{
						m_settingsEditorHeaderInclude = (string) fieldValue;
					}
				}
				else
					throw new Exception( "Invalid type for setting " + field.Name );
			}
		}

		private Type[] GetTypesInNamespace( Assembly assembly, string nameSpace )
		{
			return assembly.GetTypes().Where( t => String.Equals( t.Namespace, nameSpace, StringComparison.Ordinal ) ).ToArray();
		}

		private void ReflectSettings( Assembly assembly, string inputFilePath, List<SettingGroup> settingGroups, List<Type> enumTypes )
		{
			string filePath = Path.GetFileNameWithoutExtension( inputFilePath );
			//Type settingsType = assembly.GetType( filePath + ".Settings", false );
			//if ( settingsType == null )
			//	throw new Exception( "Settings file " + inputFilePath + " doesn't define a \"Settings\" class" );

			//ReflectType( settingsType, settings, enumTypes, "" );

			//Type[] nestedTypes = settingsType.GetNestedTypes();
			//foreach ( Type nestedType in nestedTypes )
			//	ReflectType( nestedType, settings, enumTypes, nestedType.LongName );

			Type[] nestedTypes = GetTypesInNamespace( assembly, filePath );
			if ( nestedTypes == null || nestedTypes.Length == 0 )
				throw new Exception( "Settings file " + inputFilePath + " does not define any classes" );

			foreach ( Type nestedType in nestedTypes )
			{
				if ( nestedType.IsEnum )
					continue;

				if (nestedType.Name == "GeneratorConfig")
				{
					ReflectGeneratorConfig( nestedType );
					continue;
				}

				SettingGroup settingGroup = new SettingGroup( nestedType.Name );
				settingGroups.Add( settingGroup );

				ReflectType( nestedType, enumTypes, settingGroup );
			}
		}

		//static void WriteIfChanged( List<string> lines, string outputPath )
		//{
		//	string outputText = "";
		//	foreach ( string line in lines )
		//		outputText += line + "\r\n";

		//	string fileText = "";
		//	if ( File.Exists( outputPath ) )
		//		fileText = File.ReadAllText( outputPath );

		//	int idx = fileText.IndexOf( "// ================================================================================================" );
		//	if ( idx >= 0 )
		//		outputText += "\r\n" + fileText.Substring( idx );

		//	if ( fileText != outputText )
		//		File.WriteAllText( outputPath, outputText );
		//}

		public static void WriteEnumTypes( List<string> lines, List<Type> enumTypes )
		{
			foreach ( Type enumType in enumTypes )
			{
				if ( enumType.GetEnumUnderlyingType() != typeof( int ) )
					throw new Exception( "Invalid underlying type for enum " + enumType.Name + ", must be int" );
				string[] enumNames = enumType.GetEnumNames();
				int numEnumValues = enumNames.Length;

				Array values = enumType.GetEnumValues();
				int[] enumValues = new int[numEnumValues];
				for (int i = 0; i < numEnumValues; ++i)
					enumValues[i] = (int) values.GetValue( i );

				lines.Add( "enum class " + enumType.Name );
				lines.Add( "{" );
				for (int i = 0; i < enumNames.Length; ++i)
				{
					lines.Add( "\t" + enumNames[i] + " = " + enumValues[i] + "," );
					//FieldInfo enumField = enumType.GetField( enumNames[i] );
					//EnumLabelAttribute attr = enumField.GetCustomAttribute<EnumLabelAttribute>();
					//string enumLabel = attr != null ? attr.Label : enumNames[i];
					//lines.Add( "\t" + enumLabel + "," );
				}
				lines.Add( "\r\n\tNumValues" );

				lines.Add( "};\r\n" );

				//lines.Add( "typedef EnumSettingT<" + enumType.LongName + "> " + enumType.LongName + "Setting;\r\n" );
			}
		}

		//public static void WriteEnumLabels(List<string> lines, List<Type> enumTypes)
		//{
		//	foreach(Type enumType in enumTypes)
		//	{
		//		string[] enumNames = enumType.GetEnumNames();
		//		int numEnumValues = enumNames.Length;
		//		string[] enumLabels = new string[numEnumValues];

		//		for(int i = 0; i < numEnumValues; ++i)
		//		{
		//			FieldInfo enumField = enumType.GetField(enumNames[i]);
		//			EnumLabelAttribute attr = enumField.GetCustomAttribute<EnumLabelAttribute>();
		//			enumLabels[i] = attr != null ? attr.Label : enumNames[i];
		//		}

		//		lines.Add("static const char* " + enumType.LongName + "Labels[" + numEnumValues + "] =");
		//		lines.Add("{");
		//		foreach(string label in enumLabels)
		//			lines.Add("    \"" + label + "\",");

		//		lines.Add("};\r\n");
		//	}
		//}

		static private string SettingTypeToCppType( SettingType type )
		{
			if ( type == SettingType.Bool )
				return "eParamType_bool";
			else if ( type == SettingType.Int )
				return "eParamType_int";
			else if (type == SettingType.Enum)
				return "eParamType_enum";
			else if (type == SettingType.Float)
				return "eParamType_float";
			else if (type == SettingType.String)
				return "eParamType_string";
			else if (type == SettingType.Color)
				return "eParamType_color";
			else if (type == SettingType.Float4)
				return "eParamType_float4";
			else
				throw new Exception( "Unsupported setting type" );
		}

		void GenerateHeader( List<SettingGroup> settingGroups, string outputName, string outputPath, List<Type> enumTypes )
		{
			List<string> lines = new List<string>();

			lines.Add( "#pragma once" );
			lines.Add( "" );
			//lines.Add( "#include <SettingsEditor.h>" );
			lines.Add( m_settingsEditorHeaderInclude );
			lines.Add( "" );
			lines.Add( "namespace " + outputName + "Namespace" );
			lines.Add( "{" );
			lines.Add( "" );

			WriteEnumTypes( lines, enumTypes );

			lines.Add( "struct " + outputName );
			lines.Add( "{" );

			//uint numCBSettings = 0;
			foreach ( SettingGroup settingGroup in settingGroups )
			{
				lines.Add( "\tstruct " + settingGroup.Name );
				lines.Add( "\t{" );

				foreach ( Setting setting in settingGroup.Settings )
				{
					setting.WriteDeclaration( lines );
					//if ( setting.UseAsShaderConstant )
					//	++numCBSettings;
				}

				lines.Add( "\t} m" + settingGroup.Name + ";" );
			}

			lines.Add( "}; // struct " + outputName );

			lines.Add( "" );

			lines.Add( "class " + outputName + "Wrap : public " + outputName );
			lines.Add( "{" );
			lines.Add( "public:" );
			lines.Add( "\t~" + outputName + "Wrap()" );
			lines.Add( "\t{" );
			lines.Add( "\t\tunload();" );
			lines.Add( "\t}" );

			lines.Add( "" );

			lines.Add( "\tvoid load( const char* filePath )" );
			lines.Add( "\t{" );
			lines.Add( "\t\tif ( settingsFile_ )" );
			lines.Add( "\t\t\treturn;" );
			lines.Add( "" );

			//lines.Add( "#ifdef PICO_ENGINE_CONFIG_DYNAMIC" );
			//lines.Add( "\t\tconfigFile_ = SettingsEditor::createConfigFile2( filePath );" );

			int nSettings = 0;
			foreach ( SettingGroup settingGroup in settingGroups )
			{
				foreach ( Setting setting in settingGroup.Settings )
				{
					++nSettings;
					//if ( setting.Type == SettingType.Enum )
					//	lines.Add( "\t\tconfigFile_->addParamEnum( \"" + settingGroup.LongName + "." + setting.LongName + "\", reinterpret_cast<int*>(&m" + settingGroup.LongName + "." + setting.LongName + ") );" );
					//else
					//lines.Add( "\t\tconfigFile_->addParam( \"" + settingGroup.LongName + "." + setting.LongName + "\", &m" + settingGroup.LongName + "." + setting.LongName + ");" );
				}
			}

			//lines.Add( "#else" );

			lines.Add( "\t\tvoid* paddr[" + nSettings + "];" );
			lines.Add( "\t\tSettingsEditor::e_ParamType ptype[" + nSettings + "];" );
			lines.Add( "\t\tconst char* pname[" + nSettings + "];" );

			int settingIndex = 0;
			foreach ( SettingGroup settingGroup in settingGroups )
			{
				foreach ( Setting setting in settingGroup.Settings )
				{
					lines.Add( "\t\tpaddr[" + settingIndex + "] = &m" + settingGroup.Name + "." + setting.Name + ";" );
					lines.Add( "\t\tptype[" + settingIndex + "] = SettingsEditor::" + SettingTypeToCppType( setting.Type ) + ";" );
					lines.Add( "\t\tpname[" + settingIndex + "] = \"" + setting.LongName + "\";" );
					++settingIndex;
				}
			}

			//lines.Add( "\t\tSettingsEditor::initConfigParams( filePath, paddr, ptype, " + nSettings + " )" );
			lines.Add( "\t\tsettingsFile_ = SettingsEditor::createSettingsFile( filePath, paddr, ptype, pname, " + nSettings + " );" );
			//lines.Add( "#endif // " );
			lines.Add( "\t}" );

			lines.Add( "" );

			lines.Add( "\tvoid unload()" );
			lines.Add( "\t{" );
			lines.Add( "\t\tSettingsEditor::releaseSettingsFile( settingsFile_ );" );
			lines.Add( "\t}" );

			lines.Add( "" );
			lines.Add( "private:" );
			lines.Add( "\tSettingsEditor::SettingsFile* settingsFile_ = nullptr;" );

			lines.Add( "}; // class" + outputName + "Wrap" );
			lines.Add( "" );
			lines.Add( "} // namespace " + outputName + "Namespace" );

			StringBuilder sb = new StringBuilder();
			foreach ( string line in lines )
				sb.AppendLine( line );

			string sourceCode = sb.ToString();
			System.Diagnostics.Debug.WriteLine( sourceCode );

			File.WriteAllText( outputPath, sourceCode );
			
			//if ( numCBSettings > 0 )
			//{
			//	lines.Add( "" );
			//	lines.Add( string.Format( "    struct {0}CBuffer", outputName ) );
			//	lines.Add( "    {" );

			//	uint cbSize = 0;
			//	foreach ( Setting setting in settings )
			//		setting.WriteCBufferStruct( lines, ref cbSize );

			//	lines.Add( "    };" );
			//	lines.Add( "" );
			//	lines.Add( string.Format( "    extern ConstantBuffer<{0}CBuffer> CBuffer;", outputName ) );
			//}

			//lines.Add( "" );
			//lines.Add( "    void Initialize(ID3D11Device* device);" );
			//lines.Add( "    void UpdateCBuffer(ID3D11DeviceContext* context);" );

			//lines.Add( "};" );

			//WriteIfChanged( lines, outputPath );
		}

		//static void Run(string[] args)
		//{
		//	if(args.Length < 1)
		//		throw new Exception("Invalid command-line parameters");

		//	List<Setting> settings = new List<Setting>();
		//	List<Type> enumTypes = new List<Type>();

		//	string filePath = args[0];
		//	string fileName = Path.GetFileNameWithoutExtension(filePath);

		//	Assembly compiledAssembly = CompileSettings(filePath);
		//	ReflectSettings(compiledAssembly, filePath, settings, enumTypes);

		//	string outputDir = Path.GetDirectoryName(filePath);
		//	string outputPath = Path.Combine(outputDir, fileName) + ".h";
		//	GenerateHeader(settings, fileName, outputPath, enumTypes);

		//	outputPath = Path.Combine(outputDir, fileName) + ".cpp";
		//	GenerateCPP(settings, fileName, outputPath, enumTypes);

		//	outputPath = Path.Combine(outputDir, fileName) + ".hlsl";
		//	GenerateHLSL(settings, fileName, outputPath, enumTypes);

		//	// Generate a dummy file that MSBuild can use to track dependencies
		//	outputPath = Path.Combine(outputDir, fileName) + ".deps";
		//	File.WriteAllText(outputPath, "This file is output to allow MSBuild to track dependencies");
		//}

		public void GenerateHeaderIfChanged( string filePath )
		{
			string fileName = Path.GetFileNameWithoutExtension( filePath );
			string outputDir = Path.GetDirectoryName( filePath );
			string outputPath = Path.Combine( outputDir, fileName ) + ".h";

			FileInfo srcFileInfo = new FileInfo( filePath );
			FileInfo dstFileInfo = new FileInfo( outputPath );

			// add additional version of the compiler check
			// rebuild is needed if compiler was updated
			//

			//if ( srcFileInfo.LastWriteTime >= dstFileInfo.LastWriteTime )
			{
				//ReflectSettings( filePath );
				GenerateHeader( m_reflectedSettings, fileName, outputPath, m_reflectedEnums );
			}
			//else
			//{
			//	Outputs.WriteLine( OutputMessageType.Info, "Settings file '{0} is up-to-date", filePath );
			//}
		}

		public void ReflectSettings( string filepath )
		{
			Assembly compiledAssembly = CompileSettings( filepath );

			m_reflectedSettings = new List<SettingGroup>();
			m_reflectedEnums = new List<Type>();

			ReflectSettings( compiledAssembly, filepath, m_reflectedSettings, m_reflectedEnums );
		}

		public List<SettingGroup> ReflectedSettings { get { return m_reflectedSettings; } }
		public List<Type> ReflectedEnums { get { return m_reflectedEnums; } }

		private List<SettingGroup> m_reflectedSettings = new List<SettingGroup>();
		private List<Type> m_reflectedEnums = new List<Type>();

		private string m_settingsEditorHeaderInclude = "#include <SettingsEditor.h>";
    }
}