# Makefile for building Blizzy's Toolbar

KSPDIR  := ${HOME}/.local/share/Steam/SteamApps/common/Kerbal\ Space\ Program
MANAGED := ${KSPDIR}/KSP_Data/Managed/

FILES := $(wildcard Toolbar/*/*.cs) \
         $(wildcard Toolbar/Internal/*/*.cs)

GMCS    := gmcs
GIT     := git
TAR     := tar
ZIP     := zip

all: build

info:
	@echo "== Toolbar Build Information =="
	@echo "  gmcs:    ${GMCS}"
	@echo "  git:     ${GIT}"
	@echo "  tar:     ${TAR}"
	@echo "  zip:     ${ZIP}"
	@echo "  KSP Data: ${KSPDIR}"
	@echo "================================"

build: info
	mkdir -p build
	${GMCS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine \
		-out:build/Toolbar.dll \
		${FILES}

package: build
	mkdir -p package/Toolbar/GameData/000_Toolbar/
	cp Toolbar/etc/*.tga package/Toolbar/GameData/000_Toolbar/
	cp build/Toolbar.dll package/Toolbar/GameData/000_Toolbar/
	cp README.md package/Toolbar/
	cp Toolbar/LICENSE.txt Toolbar/CHANGES.txt package/Toolbar/

tar.gz: package
	${TAR} zcf Toolbar-$(shell ${GIT} describe --tags --long --always).tar.gz package/Toolbar

zip: package
	${ZIP} -9 -r Toolbar-$(shell ${GIT} describe --tags --long --always).zip package/Toolbar

clean:
	@echo "Cleaning up build and package directories..."
	rm -rf build/ package/

install: build
	mkdir -p ${KSPDIR}/GameData/000_Toolbar/
	cp build/Toolbar.dll ${KSPDIR}/GameData/000_Toolbar/

uninstall: info
	rm -rf ${KSPDIR}/GameData/Toolbar/Plugins


.PHONY : all info build package tar.gz zip clean install uninstall
