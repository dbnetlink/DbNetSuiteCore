"use strict";
class ImageViewer extends Dialog {
    constructor(id) {
        var _a, _b, _c, _d;
        super(id);
        this.fileName = "";
        this.fileType = "";
        this.$image = (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.find("img.main-image");
        this.$video = (_b = this.$dialog) === null || _b === void 0 ? void 0 : _b.find("video.main-video");
        this.$audio = (_c = this.$dialog) === null || _c === void 0 ? void 0 : _c.find("audio.main-audio");
        this.$download = (_d = this.$dialog) === null || _d === void 0 ? void 0 : _d.find("a.download-link");
        this.button("download").on("click", () => this.download());
        this.$image.on("load", () => this.openViewer());
        this.$audio.on("loadeddata", () => this.openViewer());
        this.$video.on("loadeddata", () => this.openViewer());
    }
    /*
    public show($img: JQuery<HTMLImageElement>) {
        this.assign($img.attr("src") as string, $img.data("filename"))
    }

    public showLink(src: string) {
        this.assign(src, src.split('/').slice(-1)[0])
    }
    */
    show(src, fileName, type = "Image") {
        var _a;
        this.fileType = type;
        this.$image.hide();
        this.$video.hide();
        this.$audio.hide();
        let $element = null;
        let title = "";
        switch (type.toLowerCase()) {
            case "image":
                title = `Image Viewer (${fileName})`;
                $element = this.$image;
                break;
            case "video":
                title = `Video Player (${fileName})`;
                $element = this.$video;
                break;
            case "audio":
                title = `Audio Player (${fileName})`;
                $element = this.$audio;
                break;
        }
        if ($element != null) {
            (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.dialog("option", "title", title);
            $element.show();
            $element.removeAttr("width").removeAttr("height").removeAttr("style");
            $element.parent().removeAttr("width").removeAttr("height").removeAttr("style");
            $element.attr("src", src);
            this.fileName = fileName;
        }
    }
    openViewer() {
        var _a;
        this.open();
        const $element = (this.fileType == "Image" ? this.$image : this.$video);
        let width = $element.width();
        let height = $element.height();
        const maxWidth = Math.trunc(this.windowWidth * 0.8);
        const maxHeight = Math.trunc(this.windowHeight * 0.8);
        if ((width > maxWidth) || (height > maxHeight)) {
            const widthExcessRatio = (width / maxWidth);
            const heightExcessRatio = (height / maxHeight);
            if (widthExcessRatio > heightExcessRatio) {
                width = Math.trunc(width / widthExcessRatio);
                height = Math.trunc(height / widthExcessRatio);
                $element.width(width).css("height", "auto");
            }
            else {
                height = Math.round(height / heightExcessRatio);
                width = Math.trunc(width / heightExcessRatio);
                $element.height(height).css("width", "auto");
            }
        }
        $element.parent().width(width).height(height).css('overflow', 'hidden');
        (_a = this.$dialog) === null || _a === void 0 ? void 0 : _a.dialog("option", "width", width + 10).dialog("option", "height", height + 110);
    }
    download() {
        const link = this.$download.get(0);
        link.href = this.$image.attr("src");
        link.download = this.fileName;
        link.click();
    }
}
