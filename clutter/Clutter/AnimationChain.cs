// 
// AnimationChain.cs
//  
// Author:
//   Aaron Bockover <abockover@novell.com>
// 
// Copyright 2009 Novell, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Runtime.InteropServices;

namespace Clutter
{
    public class AnimationChain
    {
        private Animation last_animation;
        public Animation LastAnimation {
            get { return last_animation; }
        }
        
        public AnimationMode Easing { get; set; }
        public uint Duration { get; set; }
        public string Target { get; set; }
    
        private Actor actor;
        public Actor Actor {
            get { return actor; }
        }
        
        private Action<Actor> when_finished_handler;
    
        public AnimationChain (Actor actor)
        {
            if (actor == null) {
                throw new ArgumentNullException ("actor");
            }
            
            this.actor = actor;
        }
        
        public AnimationChain SetEasing (AnimationMode easing)
        {
            Easing = easing;
            return this;
        }
        
        public AnimationChain SetDuration (uint duration)
        {
            Duration = duration;
            return this;
        }
        
        public AnimationChain SetTarget (string property)
        {
            Target = property;
            return this;
        }
        
        public AnimationChain WhenFinished (Action<Actor> handler)
        {
            when_finished_handler = handler;
            return this;
        }

        public AnimationChain To<T> (T value) where T : struct
        {
            return Animate (Easing, Duration, Target, value);
        }

        public AnimationChain Animate<T> (string target, T value) where T : struct
        {
            return Animate (Easing, Duration, target, value);
        }
                        
        public AnimationChain Animate<T> (AnimationMode easing, uint duration, string property, T value) where T : struct
        {
            IntPtr result = IntPtr.Zero;
            IntPtr handle = actor.Handle;
            IntPtr prop = GLib.Marshaller.StringToPtrGStrdup (property);
            uint mode = (uint)easing;
            
            try {
                var type = value.GetType ();

                if (type == typeof (byte)) {
                    result = clutter_actor_animate_byte (handle, mode, duration, prop, 
                        (byte)Convert.ChangeType (value, typeof (byte)), IntPtr.Zero);
                } else if (type == typeof (int)) {
                    result = clutter_actor_animate_int (handle, mode, duration, prop, 
                        (int)Convert.ChangeType (value, typeof (int)), IntPtr.Zero);
                } else if (type == typeof (uint)) {
                    result = clutter_actor_animate_uint (handle, mode, duration, prop, 
                        (uint)Convert.ChangeType (value, typeof (uint)), IntPtr.Zero);
                } else if (type == typeof (double) || type == typeof (float)) {
                    result = clutter_actor_animate_double (handle, mode, duration, prop, 
                        (double)Convert.ChangeType (value, typeof (double)), IntPtr.Zero);
                } else {
                    throw new ArgumentException ("typeof(value) must be byte, int, uint, double, or float", "value");
                }
            } finally {
                GLib.Marshaller.Free (prop);
            }
            
            if (last_animation != null) {
                last_animation.Completed -= OnLastAnimationCompleted;
            }
            
            last_animation = result != IntPtr.Zero
                ? (Animation)GLib.Object.GetObject (result)
                : null;
            
            if (last_animation != null) {
                last_animation.Completed += OnLastAnimationCompleted;
            }
            
            return this;
        }
        
        private void OnLastAnimationCompleted (object o, EventArgs args)
        {
            if (when_finished_handler != null) {
                when_finished_handler (actor);
            }
        }
        
        [DllImport ("libclutter-win32-1.0-0.dll", EntryPoint="clutter_actor_animate")]
        private static extern IntPtr clutter_actor_animate_uint (IntPtr handle, uint mode,
            uint duration, IntPtr property_name, uint value, IntPtr sentinel);
        
        [DllImport ("libclutter-win32-1.0-0.dll", EntryPoint="clutter_actor_animate")]
        private static extern IntPtr clutter_actor_animate_int (IntPtr handle, uint mode,
            uint duration, IntPtr property_name, int value, IntPtr sentinel);
        
        [DllImport ("libclutter-win32-1.0-0.dll", EntryPoint="clutter_actor_animate")]
        private static extern IntPtr clutter_actor_animate_byte (IntPtr handle, uint mode,
            uint duration, IntPtr property_name, byte value, IntPtr sentinel);
        
        [DllImport ("libclutter-win32-1.0-0.dll", EntryPoint="clutter_actor_animate")]
        private static extern IntPtr clutter_actor_animate_double (IntPtr handle, uint mode,
            uint duration, IntPtr property_name, double value, IntPtr sentinel);
    }       
}
