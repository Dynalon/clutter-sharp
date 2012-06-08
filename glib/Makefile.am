assemblydir = $(prefix)/lib/clutter-sharp
READLINK="../readlink-osx"
#READLINK="readlink -e"

assembly_DATA = \
	glib-sharp.dll \
	glib-sharp.dll.config

.PHONY: patch-glib

all: patch-glib

	monodis glib-sharp.dll | awk -v self=$(srcdir)/apply-patch.awk \
		-v mode=1 -f $(srcdir)/apply-patch.awk > glib-sharp.il
	ilasm /dll /output:glib-sharp.dll glib-sharp.il
	rm glib-sharp.il

patch.dll: patch.cs glib-sharp.dll
	$(MCS) -out:$@ -target:library -nowarn:0169 -r:glib-sharp.dll $<

%.dll:
	@for lib in $(GTKSHARP_LIBS); do \
		case "$$lib" in *$@*) \
			echo "Copying $${lib:3}"; \
			cp `$(READLINK) $${lib:3}`{,.config} . ;; \
		esac; \
	done

EXTRA_DIST = $(wildcard *.cs) apply-patch.awk
CLEANFILES = $(wildcard *.dll) $(wildcard *.config)
MAINTAINERCLEANFILES = Makefile.in
