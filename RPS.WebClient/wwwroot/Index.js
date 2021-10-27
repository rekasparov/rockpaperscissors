$(() => {

    var connection;
    var round;
    var btnLetsPlay;
    var txtNickname;
    var login;
    var game;
    var score;
    var opponentSelection;
    var btnSelectionRock;
    var btnSelectionPaper;
    var btnSelectionScissors;
    var roundTitle;
    var modalTitle;
    var modalBody;
    var modalStatus;
    var modalClose;
    var ownSelection;
    var opponentSelection;

    connection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:9000/gameHub')
        .withAutomaticReconnect(5000)
        .build();

    connection.on('ServerNotAvailable', () => {
        configureLetsPlayButton(0);
    });

    connection.on('ServerAvailable', () => {
        configureLetsPlayButton(1);
    });

    connection.on('NicknameAlreadyUsing', () => {
        configureLetsPlayButton(2);
    });

    configureLetsPlayButton = (x) => {
        var buttonLabel;
        if (x == 0 || x == 2) {
            $(btnLetsPlay).addClass('disabled');
            $(btnLetsPlay).prop('disabled', true);
            if (x == 0) buttonLabel = 'Server is not available';
            else if (x == 2) {
                buttonLabel = 'Nickname is already using';
                setTimeout(() => configureLetsPlayButton(1), 3000);
            }
        }
        else if (x == 1) {
            buttonLabel = "let's play";
            $(btnLetsPlay).removeClass('disabled');
            $(btnLetsPlay).removeAttr('disabled');

        }
        $(btnLetsPlay).html(buttonLabel);
    }

    connection.on('WaitingForOpponent', () => {
        $(btnLetsPlay).html("Waiting For Opponent");
        $(btnLetsPlay).addClass('disabled');
        $(btnLetsPlay).prop('disabled', true);

        $(txtNickname).addClass('disabled');
        $(txtNickname).prop('disabled', true);
    });

    connection.on('LetsPlay', () => {
        $(login).addClass('d-none');
        $(game).removeClass('d-none');

        if (!$(txtNickname).hasClass('disabled')) {
            $(txtNickname).addClass('disabled');
            $(txtNickname).prop('disabled', true);
        }

    });

    connection.on('YouWin', (x, y, z) => {
        configureOpponentSelection(x, y, z);
    });

    connection.on('YouLost', (x, y, z) => {
        configureOpponentSelection(x, y, z);
    });

    connection.on('NoWin', (x, y, z) => {
        configureOpponentSelection(x, y, z);
    });

    configureOpponentSelection = (x, y, z) => {
        $(score).html((x + ' - ' + y));
        if (z == 0) $(opponentSelection).prop('src', 'img/rock.png');
        else if (z == 1) $(opponentSelection).prop('src', 'img/paper.png');
        else if (z == 2) $(opponentSelection).prop('src', 'img/scissors.png');
    };

    connection.on('NewRound', () => {
        round++;

        if (round == 2) $(roundTitle).html('Second Round');
        else if (round == 3) $(roundTitle).html('Third Round');
        else if (round == 4) $(roundTitle).html('Fourth Round');
        else if (round == 5) $(roundTitle).html('Last Round');

        openSelectionButton();
    });

    openSelectionButton = () => {
        $(btnSelectionRock).removeClass('disabled');
        $(btnSelectionPaper).removeClass('disabled');
        $(btnSelectionScissors).removeClass('disabled');

        $(btnSelectionRock).removeAttr('disabled');
        $(btnSelectionPaper).removeAttr('disabled');
        $(btnSelectionScissors).removeAttr('disabled');
    };

    closeSelectionButton = () => {
        $(btnSelectionRock).addClass('disabled');
        $(btnSelectionPaper).addClass('disabled');
        $(btnSelectionScissors).addClass('disabled');

        $(btnSelectionRock).prop('disabled', true);
        $(btnSelectionPaper).prop('disabled', true);
        $(btnSelectionScissors).prop('disabled', true);
    };

    connection.on('GameFinish', (x, y) => {
        var innerHtml = '';

        innerHtml += '<div class="pricing-header px-3 py-3 pb-md-4 mx-auto text-center">';
        innerHtml += '<h1 class="display-4">Score</h1>';
        innerHtml += '<p class="lead text-center">';
        innerHtml += '<h1 id="score">' + x + ' - ' + y + '</h1>';
        innerHtml += '</p>';
        innerHtml += '</div>';

        $(modalTitle).html('Thanks for playing');
        $(modalBody).html(innerHtml);
        $(modalStatus).modal('show');

        disposeGame();
    });

    disposeGame = () => {
        round = 1;

        $(score).html('0 - 0');

        $(roundTitle).html('First Round');

        openSelectionButton();
    };

    startConnection = async () => {
        await connection.start();
    };

    $(document).ready(() => {

        init();

        $(btnLetsPlay).on('click', async () => {
            if ($(txtNickname).val().trim() == '') {
                $(btnLetsPlay).addClass('disabled');
                $(btnLetsPlay).prop('disabled', true);
                $(btnLetsPlay).html("Nickname is not be null");

                setTimeout(() => configureLetsPlayButton(1), 3000);
            }
            else await connection.invoke('MayIPlay', $(txtNickname).val().trim());
        });

        $(modalClose).on('click', () => {
            $(ownSelection).prop('src', 'img/rps.png');
            $(opponentSelection).prop('src', 'img/rps.png');
        });

    });

    btnSelection_click = async (e) => {
        var selection = parseInt($(e.target).data('id'));

        if (selection == 0) $(ownSelection).prop('src', 'img/rock.png');
        else if (selection == 1) $(ownSelection).prop('src', 'img/paper.png');
        else if (selection == 2) $(ownSelection).prop('src', 'img/scissors.png');

        $(opponentSelection).prop('src', 'img/rps.png');

        closeSelectionButton();

        await connection.invoke('MySelection', selection);
    };

    init = () => {
        round = 1;

        btnLetsPlay = $('#btnLetsPlay');
        txtNickname = $('#txtNickname');
        login = $('#login');
        game = $('#game');
        score = $('#score');
        opponentSelection = $('#opponentSelection');
        btnSelectionRock = $('#btnSelectionRock');
        btnSelectionPaper = $('#btnSelectionPaper');
        btnSelectionScissors = $('#btnSelectionScissors');
        roundTitle = $('#roundTitle');
        modalTitle = $('#modalTitle');
        modalBody = $('#modalBody');
        modalStatus = $('#modalStatus');
        modalClose = $('#modalClose');
        ownSelection = $('#ownSelection');
        opponentSelection = $('#opponentSelection');

        $(btnSelectionRock).on('click', btnSelection_click);
        $(btnSelectionPaper).on('click', btnSelection_click);
        $(btnSelectionScissors).on('click', btnSelection_click);
    };

    startConnection();
});