# Makefile for building Blizzy's Toolbar

KSPDIR  := ${HOME}/.local/share/Steam/SteamApps/common/Kerbal\ Space\ Program
MANAGED := ${KSPDIR}/KSP_Data/Managed/

FILES := $(wildcard Toolbar/*/*.cs) \
         $(wildcard Toolbar/Internal/*/*.cs)

GMCS    := gmcs
GIT     := git
TAR     := tar
ZIP     := zip

VERSION := $(shell ${GIT} describe --tags --always)

all: build

info:
	@echo "== Toolbar Build Information =="
	@echo "  gmcs:    ${GMCS}"
	@echo "  git:     ${GIT}"
	@echo "  tar:     ${TAR}"
	@echo "  zip:     ${ZIP}"
	@echo "  KSP Data: ${KSPDIR}"
	@echo "================================"

build: build/Toolbar.dll

build/%.dll: ${FILES}
	mkdir -p build
	${GMCS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine \
		-out:$@ \
		${FILES}

package: build ${FILES}
	mkdir -p package/Toolbar/GameData/000_Toolbar/
	cp Toolbar/etc/*.tga package/Toolbar/GameData/000_Toolbar/
	cp build/Toolbar.dll package/Toolbar/GameData/000_Toolbar/
	cp README.md package/Toolbar/
	cp Toolbar/LICENSE.txt Toolbar/CHANGES.txt \
		package/Toolbar/GameData/

tar.gz: package Toolbar-${VERSION}.tar.gz

%.tar.gz: package/Toolbar/GameData/000_Toolbar/%.dll
	${TAR} zcf $@ package/Toolbar

tar.gz: package Toolbar-${VERSION}.zip

%.zip: package/Toolbar/GameData/000_Toolbar/%.dll
	${ZIP} -9 -r $@ package/Toolbar

clean:
	@echo "Cleaning up build and package directories..."
	rm -rf build/ package/

install: build
	mkdir -p ${KSPDIR}/GameData/000_Toolbar/
	cp build/Toolbar.dll ${KSPDIR}/GameData/000_Toolbar/
	cp Toolbar/etc/*.tga ${KSPDIR}/GameData/000_Toolbar/

uninstall: info
	rm -rf ${KSPDIR}/GameData/000_Toolbar


.PHONY : all info build package tar.gz zip clean install uninstall
