// GtkSharp.Generation.OpaqueGen.cs - The Opaque Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class OpaqueGen : HandleBase {

		public OpaqueGen (XmlElement ns, XmlElement elem) : base (ns, elem) {}
	
		public override string FromNative(string var, bool owned)
		{
			return var + " == IntPtr.Zero ? null : (" + QualifiedName + ") GLib.Opaque.GetOpaque (" + var + ", typeof (" + QualifiedName + "), " + (owned ? "true" : "false") + ")";
		}

		private bool DisableRawCtor {
			get {
				return Elem.HasAttribute ("disable_raw_ctor");
			}
		}

		public override void Generate (GenerationInfo gen_info)
		{
			gen_info.CurrentType = Name;

			StreamWriter sw = gen_info.Writer = gen_info.OpenStream (Name);

			sw.WriteLine ("namespace " + NS + " {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();

			sw.WriteLine ("#region Autogenerated code");

			SymbolTable table = SymbolTable.Table;

			Method ref_, unref, dispose;
			GetSpecialMethods (out ref_, out unref, out dispose);

			if (IsDeprecated)
				sw.WriteLine ("\t[Obsolete]");
			sw.Write ("\t{0}{1}class " + Name, IsInternal ? "internal " : "public ", IsAbstract ? "abstract " : String.Empty);
			string cs_parent = table.GetCSType(Elem.GetAttribute("parent"));
			if (cs_parent != "")
				sw.Write (" : " + cs_parent);
			else
				sw.Write (" : GLib.Opaque");

			foreach (string iface in managed_interfaces) {
				if (Parent != null && Parent.Implements (iface))
					continue;
				sw.Write (", " + iface);
			}

			sw.WriteLine (" {");
			sw.WriteLine ();
            
			GenFields (gen_info);
			GenMethods (gen_info, null, null);
			GenCtors (gen_info);

			if (ref_ != null) {
				ref_.GenerateImport (sw);
				sw.WriteLine ("\t\tprotected override void Ref (IntPtr raw)");
				sw.WriteLine ("\t\t{");
				sw.WriteLine ("\t\t\tif (!Owned) {");
				sw.WriteLine ("\t\t\t\t" + ref_.CName + " (raw);");
				sw.WriteLine ("\t\t\t\tOwned = true;");
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ("\t\t}");
				sw.WriteLine ();

				if (ref_.IsDeprecated) {
					sw.WriteLine ("\t\t[Obsolete(\"" + QualifiedName + " is now refcounted automatically\")]");
					if (ref_.ReturnType == "void")
						sw.WriteLine ("\t\tpublic void Ref () {}");
					else
						sw.WriteLine ("\t\tpublic " + Name + " Ref () { return this; }");
					sw.WriteLine ();
				}
			}

			bool finalizer_needed = false;

			if (unref != null) {
				unref.GenerateImport (sw);
				sw.WriteLine ("\t\tprotected override void Unref (IntPtr raw)");
				sw.WriteLine ("\t\t{");
				sw.WriteLine ("\t\t\tif (Owned) {");
				sw.WriteLine ("\t\t\t\t" + unref.CName + " (raw);");
				sw.WriteLine ("\t\t\t\tOwned = false;");
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ("\t\t}");
				sw.WriteLine ();

				if (unref.IsDeprecated) {
					sw.WriteLine ("\t\t[Obsolete(\"" + QualifiedName + " is now refcounted automatically\")]");
					sw.WriteLine ("\t\tpublic void Unref () {}");
					sw.WriteLine ();
				}	
				finalizer_needed = true;
			}

			if (dispose != null) {
				dispose.GenerateImport (sw);
				sw.WriteLine ("\t\tprotected override void Free (IntPtr raw)");
				sw.WriteLine ("\t\t{");
				sw.WriteLine ("\t\t\t" + dispose.CName + " (raw);");
				sw.WriteLine ("\t\t}");
				sw.WriteLine ();

				if (dispose.IsDeprecated) {
					sw.WriteLine ("\t\t[Obsolete(\"" + QualifiedName + " is now freed automatically\")]");
					sw.WriteLine ("\t\tpublic void " + dispose.Name + " () {}");
					sw.WriteLine ();
				}	
				finalizer_needed = true;
			}

			if (finalizer_needed) {
				sw.WriteLine ("\t\tclass FinalizerInfo {");
				sw.WriteLine ("\t\t\tIntPtr handle;");
				sw.WriteLine ();
				sw.WriteLine ("\t\t\tpublic FinalizerInfo (IntPtr handle)");
				sw.WriteLine ("\t\t\t{");
				sw.WriteLine ("\t\t\t\tthis.handle = handle;");
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ();
				sw.WriteLine ("\t\t\tpublic bool Handler ()");
				sw.WriteLine ("\t\t\t{");
				if (dispose != null)
					sw.WriteLine ("\t\t\t\t{0} (handle);", dispose.CName);
				else if (unref != null)
					sw.WriteLine ("\t\t\t\t{0} (handle);", unref.CName);
				sw.WriteLine ("\t\t\t\treturn false;");
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ("\t\t}");
				sw.WriteLine ();
				sw.WriteLine ("\t\t~{0} ()", Name);
				sw.WriteLine ("\t\t{");
				sw.WriteLine ("\t\t\tif (!Owned)");
				sw.WriteLine ("\t\t\t\treturn;");
				sw.WriteLine ("\t\t\tFinalizerInfo info = new FinalizerInfo (Handle);");
				sw.WriteLine ("\t\t\tGLib.Timeout.Add (50, new GLib.TimeoutHandler (info.Handler));");
				sw.WriteLine ("\t\t}");
				sw.WriteLine ();
			}

#if false
			Method copy = Methods ["Copy"] as Method;
			if (copy != null && copy.Parameters.Count == 0) {
				sw.WriteLine ("\t\tprotected override GLib.Opaque Copy (IntPtr raw)");
				sw.WriteLine ("\t\t{");
				sw.WriteLine ("\t\t\tGLib.Opaque result = new " + QualifiedName + " (" + copy.CName + " (raw));");
				sw.WriteLine ("\t\t\tresult.Owned = true;");
				sw.WriteLine ("\t\t\treturn result;");
				sw.WriteLine ("\t\t}");
				sw.WriteLine ();
			}
#endif
			sw.WriteLine ("#endregion");
			
			AppendCustom(sw, gen_info.CustomDir);

			sw.WriteLine ("\t}");
			sw.WriteLine ("}");

			sw.Close ();
			gen_info.Writer = null;
			Statistics.OpaqueCount++;
		}

		void GetSpecialMethods (out Method ref_, out Method unref, out Method dispose)
		{
			ref_ = CheckSpecialMethod (GetMethod ("Ref"));
			unref = CheckSpecialMethod (GetMethod ("Unref"));

			dispose = GetMethod ("Free");
			if (dispose == null) {
				dispose = GetMethod ("Destroy");
				if (dispose == null)
					dispose = GetMethod ("Dispose");
			}
			dispose = CheckSpecialMethod (dispose);
		}

		Method CheckSpecialMethod (Method method)
		{
			if (method == null)
				return null;
			if (method.ReturnType != "void" &&
			    method.ReturnType != QualifiedName)
				return null;
			if (method.Signature.ToString () != "")
				return null;

			methods.Remove (method.Name);
			return method;
		}

		protected override void GenCtors (GenerationInfo gen_info)
		{
			if (!DisableRawCtor) {
				gen_info.Writer.WriteLine("\t\tpublic " + Name + "(IntPtr raw) : base(raw) {}");
				gen_info.Writer.WriteLine();
			}

			base.GenCtors (gen_info);
		}

	}
}

