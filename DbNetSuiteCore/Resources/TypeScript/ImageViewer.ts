class ImageViewer extends Dialog {
    $image: JQuery<HTMLImageElement>;
    $download: JQuery<HTMLAnchorElement>;
    fileName = "";
    constructor(id: string) {
        super(id);
        this.$image = this.$dialog?.find("img.main-image") as JQuery<HTMLImageElement>;
        this.$download = this.$dialog?.find("a.download-link") as JQuery<HTMLAnchorElement>;
        this.button("download").on("click", () => this.download());
        this.$image.on("load", () => this.openViewer())
    }

    public show($img: JQuery<HTMLImageElement>) {
        this.$image.removeAttr("width").removeAttr("height").removeAttr("style");
        this.$image.parent().removeAttr("width").removeAttr("height").removeAttr("style");
        this.$image.attr("src", $img.attr("src") as string);
        this.fileName = $img.data("filename");
    }

    private openViewer(): void {
        this.open();
        let width = this.$image.width() as number;
        let height = this.$image.height() as number;
        const maxWidth = Math.trunc(this.windowWidth * 0.8);
        const maxHeight = Math.trunc(this.windowHeight * 0.8);

        if ((width > maxWidth) || (height > maxHeight)) {
            const widthExcessRatio = (width / maxWidth);
            const heightExcessRatio = (height / maxHeight);

            if (widthExcessRatio > heightExcessRatio) {
                width = Math.trunc(width / widthExcessRatio);
                height = Math.trunc(height / widthExcessRatio);
                this.$image.width(width).css("height", "auto");
            }
            else {
                height = Math.round(height / heightExcessRatio);
                width = Math.trunc(width / heightExcessRatio);
                this.$image.height(height).css("width", "auto");
            }
        }

        this.$image.parent().width(width).height(height).css('overflow', 'hidden');
        this.$dialog?.dialog("option", "width", width + 10).dialog("option", "height", height + 110);
    }

    private download() {
        const link = this.$download.get(0) as HTMLAnchorElement;
        link.href = this.$image.attr("src") as string;
        link.download = this.fileName;
        link.click();
    }
}