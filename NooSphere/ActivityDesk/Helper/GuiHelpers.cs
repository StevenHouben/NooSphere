/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Microsoft.Surface.Presentation;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Threading;

namespace ActivityDesk.Helper
{
	/// <summary>
	/// Contains utility methods for searching through the visual tree.
	/// </summary>
	/// <remarks>
	/// Written by Isak Savo, isak.savo@gmail.com
	/// </remarks>
	public sealed class GuiHelpers
	{
		/// <summary>
		/// Gets the first parent object in the visual tree that is of the templated type.
		/// </summary>
		/// <typeparam name="T">The type of the parent that is wanted</typeparam>
		/// <param name="obj">The object whose parent hierarchy should be searched.</param>
		/// <param name="exactTypeMatch">if set to <c>true</c> only objects of the exact type (i.e. not classes inheriting from it) is returned.</param>
		/// <returns>The first object in the hierarchy above that is of the specified type (or subtype, depending on the <paramref name="exactTypeMatch"/>)</returns>
		public static T GetParentObject<T>(DependencyObject obj, bool exactTypeMatch) where T : DependencyObject
		{
			try
			{
				while (obj != null &&
					(exactTypeMatch ? (obj.GetType() != typeof(T)) : !(obj is T)))
				{
					if (obj is Visual || obj is Visual3D)
					{
						obj = VisualTreeHelper.GetParent(obj) as DependencyObject;
					}
					else
					{
						// If we're in Logical Land then we must walk 
						// up the logical tree until we find a 
						// Visual/Visual3D to get us back to Visual Land.
						obj = LogicalTreeHelper.GetParent(obj) as DependencyObject;
					}
				}
				return obj as T;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return null;
			}
		}

		/// <summary>
		/// Gets the first parent object in the visual tree that is of the templated type.
		/// </summary>
		/// <typeparam name="T">The type of the parent that is wanted</typeparam>
		/// <param name="obj">The object whose parent hierarchy should be searched.</param>
		/// <returns>The first object in the hierarchy above that is of the specified type</returns>
		public static T GetParentObject<T>(DependencyObject obj) where T : DependencyObject
		{
			return GetParentObject<T>(obj, false);
		}

		/// <summary>
		/// Gets the first child object in the visual tree that is of the templated type.
		/// </summary>
		/// <typeparam name="T">The type of the child that is wanted</typeparam>
		/// <param name="obj">The object whose child hierarchy should be searched.</param>
		/// <returns>The first object in the hierarchy below that is of the specified type</returns>
		public static T GetChildObject<T>(DependencyObject obj) where T : class
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child is T)
					return child as T;
				else
				{
					child = GetChildObject<T>(child) as DependencyObject;
					if (child is T)
						return child as T;
				}
			}
			return null;
		}
	}
}

