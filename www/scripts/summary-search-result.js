let showSearchingResult = (resultObj) => {
    //console.log(resultObj);
    let summarySearchContainer = document.querySelector('#main');
    let resultContainer = document.querySelector('#result-container');

    summarySearchContainer.classList.toggle('hidden', true);
    resultContainer.classList.toggle('hidden', false);

    let resultNode = document.querySelector('.canClone');

    let filePathList = [];
    resultObj.forEach(element => {
        if(!filePathList.includes(element.FilePath) )
            filePathList.push(element.FilePath);
    } );

    filePathList.sort();
    filePathList.forEach(filePath => {
        let clone = resultNode.cloneNode(true);
        clone.classList.remove('canClone');
        clone.classList.remove('hidden');

        let contents = resultObj.filter(predicate => predicate.FilePath === filePath);
        let rows = [];
        contents.forEach(content => {
            let rowContent = { row: content.RowIndex, cells: content.Content.split('\t'), sheetName: content.SheetName };
            rows.push(rowContent);
        } );
        for(let i = 0; i < rows.length; i++){
            let row = rows[i];

            let resultContentNode = i === 0 ? clone.querySelector('.result-content') : clone.querySelector('.result-content').cloneNode('true');
            let containers = Array.from(resultContentNode.getElementsByClassName('result-value-container') );

            if(Array.from(clone.children).includes(resultContentNode) === false)
                clone.appendChild(resultContentNode);

            let contentInfo = resultContentNode.querySelector('.result-content-info');
            contentInfo.setAttribute('title', `Relatório: ${row.sheetName}\nLinha: ${row.row}\n\nClique para abrir o relatório.`);
            contentInfo.addEventListener('click', () => { openExcelFile({filePath: filePath}) } );

            for(let j = 0; j < containers.length; j++){
                let container = containers[j];
                let spanNode = container.querySelector('span');

                if(j === containers.length - 1)
                {
                    spanNode.innerText = `R$ ${row.cells[j] }`;
                    continue;
                }

                spanNode.innerText = row.cells[j];
            }
        }
        clone.querySelector('.result-title').innerText = filePath;
        resultContainer.appendChild(clone);
    } )
}

let canOpenFile = true;

function openExcelFile(requestObj){
    if(!canOpenFile)
        return;

    canOpenFile = false;
    let req = new XMLHttpRequest();
    req.open('POST', `${window.location.origin}/summaryOpen`);
    req.addEventListener('loadend', () => {
        canOpenFile = true;
    } );

    req.send(JSON.stringify(requestObj) );
}