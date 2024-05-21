class ImageViewer extends Dialog {
    $image: JQuery<HTMLImageElement>;
    $video: JQuery<HTMLVideoElement>;
    $audio: JQuery<HTMLAudioElement>;
    $download: JQuery<HTMLAnchorElement>;
    fileName = "";
    fileType = "";

    constructor(id: string) {
        super(id);
        this.$image = this.$dialog?.find("img.main-image") as JQuery<HTMLImageElement>;
        this.$video = this.$dialog?.find("video.main-video") as JQuery<HTMLVideoElement>;
        this.$audio = this.$dialog?.find("audio.main-audio") as JQuery<HTMLAudioElement>;
        this.$download = this.$dialog?.find("a.download-link") as JQuery<HTMLAnchorElement>;
        this.button("download").on("click", () => this.download());
        this.$image.on("load", () => this.openViewer())
        this.$audio.on("loadeddata", () => this.openViewer())
        this.$video.on("loadeddata", () => this.openViewer())
    }
    /*
    public show($img: JQuery<HTMLImageElement>) {
        this.assign($img.attr("src") as string, $img.data("filename"))
    }

    public showLink(src: string) {
        this.assign(src, src.split('/').slice(-1)[0])
    }
    */

    public show(src: string, fileName: string, type = "Image") {
        this.fileType = type;
        this.$image.hide();
        this.$video.hide();
        this.$audio.hide();
        let $element = null;
        let title = "";
        switch (type.toLowerCase()) {
            case "image":
                title = `Image Viewer (${fileName})`;
                $element = this.$image as JQuery<HTMLElement>;
                break;
            case "video":
                title = `Video Player (${fileName})`;
                $element = this.$video as JQuery<HTMLElement>;
                break;
            case "audio":
                title = `Audio Player (${fileName})`;
                $element = this.$audio as JQuery<HTMLElement>;
                break;
        }

        if ($element != null) {
            this.$dialog?.dialog("option", "title", title)
            $element.show();
            $element.removeAttr("width").removeAttr("height").removeAttr("style");
            $element.parent().removeAttr("width").removeAttr("height").removeAttr("style");
            $element.attr("src", src);
            this.fileName = fileName;
        }
    }

    private openViewer(): void {
        this.open();

        const $element = (this.fileType == "Image" ? this.$image : this.$video) as JQuery<HTMLElement>;
        let width = $element.width() as number;
        let height = $element.height() as number;
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
        this.$dialog?.dialog("option", "width", width + 10).dialog("option", "height", height + 110);
    }

    private download() {
        const link = this.$download.get(0) as HTMLAnchorElement;
        link.href = this.$image.attr("src") as string;
        link.download = this.fileName;
        link.click();
    }
}