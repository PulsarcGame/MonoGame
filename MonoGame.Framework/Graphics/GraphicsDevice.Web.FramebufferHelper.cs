﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using static WebHelper;
using static Retyped.dom;
using glc = Retyped.webgl2.WebGL2RenderingContext;

namespace Microsoft.Xna.Framework.Graphics
{
    // ARB_framebuffer_object implementation
    partial class GraphicsDevice
    {
        internal class FramebufferHelper
        {
            private static FramebufferHelper _instance;

            public static FramebufferHelper Create(GraphicsDevice gd)
            {
                if (gd.GraphicsCapabilities.SupportsFramebufferObjectARB || gd.GraphicsCapabilities.SupportsFramebufferObjectEXT)
                {
                    _instance = new FramebufferHelper(gd);
                }
                else
                {
                    throw new PlatformNotSupportedException(
                        "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                        "Try updating your graphics drivers.");
                }

                return _instance;
            }

            public static FramebufferHelper Get()
            {
                if (_instance == null)
                    throw new InvalidOperationException("The FramebufferHelper has not been created yet!");
                return _instance;
            }

            public bool SupportsInvalidateFramebuffer { get; private set; }

            public bool SupportsBlitFramebuffer { get; private set; }

            internal FramebufferHelper(GraphicsDevice graphicsDevice)
            {
                this.SupportsBlitFramebuffer = true;
                this.SupportsInvalidateFramebuffer = true;
            }

            internal virtual void GenRenderbuffer(out WebGLRenderbuffer renderbuffer)
            {
                renderbuffer = gl.createRenderbuffer();
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindRenderbuffer(WebGLRenderbuffer renderbuffer)
            {
                gl.bindRenderbuffer(glc.RENDERBUFFER, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void DeleteRenderbuffer(WebGLRenderbuffer renderbuffer)
            {
                gl.deleteRenderbuffer(renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void RenderbufferStorageMultisample(double samples, double internalFormat, double width, double height)
            {
                gl.renderbufferStorage(glc.RENDERBUFFER, internalFormat, width, height);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenFramebuffer(out WebGLFramebuffer framebuffer)
            {
                framebuffer = gl.createFramebuffer();
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindFramebuffer(WebGLFramebuffer framebuffer)
            {
                gl.bindFramebuffer(glc.FRAMEBUFFER, framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindReadFramebuffer(WebGLFramebuffer readFramebuffer)
            {
                gl.bindFramebuffer(gl.READ_FRAMEBUFFER, readFramebuffer);
                GraphicsExtensions.CheckGLError();
            }

            static readonly double[] FramebufferAttachements = {
                glc.COLOR_ATTACHMENT0,
                glc.DEPTH_ATTACHMENT,
                glc.STENCIL_ATTACHMENT,
            };

            internal virtual void InvalidateDrawFramebuffer()
            {
                Debug.Assert(this.SupportsInvalidateFramebuffer);
                gl.invalidateFramebuffer(glc.FRAMEBUFFER, FramebufferAttachements);
            }

            internal virtual void InvalidateReadFramebuffer()
            {
                Debug.Assert(this.SupportsInvalidateFramebuffer);
                gl.invalidateFramebuffer(glc.FRAMEBUFFER, FramebufferAttachements);
            }

            internal virtual void DeleteFramebuffer(WebGLFramebuffer framebuffer)
            {
                gl.deleteFramebuffer(framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void FramebufferTexture2D(double attachement, double target, WebGLTexture texture, double level = 0, double samples = 0)
            {
                gl.framebufferTexture2D(glc.FRAMEBUFFER, attachement, target, texture, level);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void FramebufferRenderbuffer(double attachement, WebGLRenderbuffer renderbuffer, double level = 0)
            {
                gl.framebufferRenderbuffer(glc.FRAMEBUFFER, attachement, glc.RENDERBUFFER, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenerateMipmap(double target)
            {
                gl.generateMipmap(target);
                GraphicsExtensions.CheckGLError();

            }

            internal virtual void BlitFramebuffer(double iColorAttachment, double width, double height)
            {
                gl.readBuffer(glc.COLOR_ATTACHMENT0 + iColorAttachment);
                GraphicsExtensions.CheckGLError();
                gl.drawBuffers(new[] { glc.COLOR_ATTACHMENT0 + iColorAttachment });
                GraphicsExtensions.CheckGLError();
                gl.blitFramebuffer(0, 0, width, height, 0, 0, width, height, glc.COLOR_BUFFER_BIT, glc.NEAREST);
                GraphicsExtensions.CheckGLError();

            }

            /*internal virtual void CheckFramebufferStatus()
            {
                var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (status != FramebufferErrorCode.FramebufferComplete)
                {
                    string message = "Framebuffer Incomplete.";
                    switch (status)
                    {
                        case FramebufferErrorCode.FramebufferIncompleteAttachment: message = "Not all framebuffer attachment points are framebuffer attachment complete."; break;
                        case FramebufferErrorCode.FramebufferIncompleteMissingAttachment: message = "No images are attached to the framebuffer."; break;
                        case FramebufferErrorCode.FramebufferUnsupported: message = "The combination of internal formats of the attached images violates an implementation-dependent set of restrictions."; break;
                        case FramebufferErrorCode.FramebufferIncompleteMultisample: message = "Not all attached images have the same number of samples."; break;
                    }
                    throw new InvalidOperationException(message);
                }
            }*/
        }
    }
}
