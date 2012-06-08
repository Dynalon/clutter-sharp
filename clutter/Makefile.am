SUBDIRS = Cogl Clutter .

ASSEMBLY_NAME = clutter-sharp

SOURCES = \
	Clutter/KeyEvent.cs \
	Clutter/CrossingEvent.cs \
	Clutter/Application.cs \
	Clutter/Source.cs \
	Clutter/Timeout.cs \
	Clutter/ScrollEvent.cs \
	Clutter/AnimationChain.cs \
	Clutter/StageStateEvent.cs \
	Clutter/Event.cs \
	Clutter/Idle.cs \
	Clutter/MotionEvent.cs \
	Clutter/ButtonEvent.cs \
	Cogl/Matrix.cs \
	Cogl/General.cs \
	Cogl/GL.cs

GENERATED_DIRS = $(builddir)/Clutter $(builddir)/Cogl

CUSTOMS = \
	Clutter/BehaviourPath.custom \
	Clutter/Stage.custom \
	Clutter/Smoothstep.custom \
	Clutter/Color.custom \
	Clutter/Knot.custom \
	Clutter/Actor.custom \
	Clutter/ActorBox.custom \
	Clutter/Rectangle.custom \
	Clutter/Script.custom \
	Clutter/Group.custom \
	Clutter/Timeline.custom \
	Clutter/Ramp.custom \
	Clutter/Texture.custom \
	Clutter/Sine.custom \
	Clutter/Exp.custom \
	Cogl/Path.custom \
	Cogl/Angle.custom \
	Cogl/Fixed.custom

EXTRA_DIST =
CLEANFILES =

TARGET = \
	$(ASSEMBLY_NAME).dll \
	$(ASSEMBLY_NAME).dll.config \
	$(ASSEMBLY_NAME).dll.mdb

include $(top_srcdir)/build/assembly.mk

MAINTAINERCLEANFILES = Makefile.in
