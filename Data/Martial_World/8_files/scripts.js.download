$(function () {
    var isMobile = screen.width <= 768;

    makeAjaxSearch($('#hot-genre-select'), $('#intro-index').find('.index-intro'), 'hot');
    makeAjaxSearch($('#new-genre-select'), $('#list-index').find('.index-intro'), 'latest');

    function makeAjaxSearch($select, $el, type) {
        $select.on('change', function () {
            var url = ajaxSearchUrl;
            var val = $(this).val();
            $.ajax({
                method: "GET",
                url: url,
                data: {
                    type: type,
                    genre: val
                }
            }).done(function (rs) {
                $el.fadeOut(50, function () {
                    $el.html('');
                    $el.fadeIn(50, function () {
                        $el.html(rs);
                        reShowChapterText();
                    });
                });
            });
        });
    }

    function reShowChapterText() {
        $('.chapter-text').each(function (k, v) {
            if (isMobile) {
                var chapterText = $(this).html();
                chapterText = chapterText.replace('Chapter ', 'C');
                chapterText = chapterText.replace('Volume ', 'V');
                $(this).html(chapterText);
            }
        });
    }

    reShowChapterText();

    $('#page_jump').bind('click', function (e) { e.stopPropagation() });
});

function chapterJump(novelId, currentChapterId, currentChapterSlug) {
    $(".chapter-nav").on("click", "button.chapter_jump", function () {
        $("button.chapter_jump").attr("disabled", !0);
        $.get(ajaxChapterOptionUrl, {
            novelId: novelId,
            currentChapterId: currentChapterId,
        }).done(function (a) {
            $(".chapter_jump").replaceWith(a);
            $('.chapter_jump').val(currentChapterSlug);
            $('.chapter_jump').on('change', function () {
                window.location = $(this).val();
            });
        })
    });
}