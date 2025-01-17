let targetMouseElement = undefined;
let exportBtn = document.querySelector('#summary-export');

function onSummaryGenerate(request){
    generateButton.disabled = false;
    if(request.status !== 200){
        alert(decodeURI('Não foi possível gerar o relatório.') );
        return;
    }

    let resultContainer = document.querySelector('#summary-result');
    let result = JSON.parse(request.responseText);
    let rows = result.Rows.sort((a, b) => a.NF - b.NF);

    document.querySelector('#summary-export').setAttribute('output_folder', result.OutputFolder);

    rows.forEach(row => {
        let rowElement = document.querySelector('.summary-row-model').cloneNode(true);
        setRowCapabilities(rowElement, row);
        resultContainer.appendChild(rowElement);
    } );

    setColumnInputCapabilities(resultContainer);
    
    toggleSummaryRequestFormVisibility(true);
    toggleSummaryResultVisibility(false);
}

function setRowCapabilities(rowElement, row){
    let columns = Array.from(rowElement.querySelectorAll('.summary-column') );
    let btns = Array.from(rowElement.querySelector('.summary-column.btns').children);
    let rowContent = [row.EmissionDate, row.NF, row.Client, row.City, row.Volumes, row.Weight, row.Value];
    let resultContainer = document.querySelector('#summary-result');
    columns = columns.filter(column => column.classList.contains('btns') === false);

    let mouseIsDown = false;

    btns.find(btn => btn.classList.contains('button-row-move') ).addEventListener('mousedown', (eventInfo) => {
        targetMouseElement = {
            element: rowElement,
            targetElement: rowElement,
            initialY: eventInfo.screenY + window.scrollY
        }
       
    } );
    window.addEventListener('mouseup', (eventInfo) => {
        if(targetMouseElement === undefined)
            return;

        let rows = Array.from(document.querySelectorAll('.summary-row') );

        if(targetMouseElement.element !== targetMouseElement.targetElement){
            sortSummaryRows(rows.indexOf(targetMouseElement.element), rows.indexOf(targetMouseElement.targetElement) );

            if(targetMouseElement.element.getAttribute('style') !== null)
                targetMouseElement.element.removeAttribute('style');
        }
        else if(targetMouseElement.element.getAttribute('style') !== null)
            targetMouseElement.element.removeAttribute('style');
      
        rows.forEach(row => { row.classList.toggle('to-replace', false); } );
        targetMouseElement = undefined;
    } );

    btns.find(btn => btn.classList.contains('button-row-delete') ).addEventListener('click', () => { rowElement.remove(); } );
    btns.find(btn => btn.classList.contains('button-row-insert') ).addEventListener('click', () => {
        let clone = document.querySelector('.summary-row-model').cloneNode(true);
        setRowCapabilities(clone, {EmissionDate: '', NF: '', Client: '', City: '', Volumes: '', Weight: '', Value: ''} );
        setColumnInputCapabilities(clone);
        clone.classList.toggle('hidden', false);
        clone.classList.toggle('summary-row-model', false);
        resultContainer.insertBefore(clone, btns.find(btn => btn.classList.contains('button-row-insert') ).parentElement.parentElement.nextSibling);
    } );

    for(let i = 0; i < rowContent.length; i++){
        let columnContentElement = columns[i].querySelector('span');
        columnContentElement.innerText = rowContent[i];

        columns[i].addEventListener('click', () => {     
            let columnInputElement = columns[i].querySelector('textarea');
            columnInputElement.value = columnContentElement.innerText;
            if(columnInputElement.hidden ){
                let style = getComputedStyle(columnContentElement);
                let heightStr = columnInputElement.value.trim().length === 0 ? (parseInt(getComputedStyle(columns[i] ).height) * 2) + 'px' : style.height;

                columnInputElement.style.height = heightStr;

                columnContentElement.hidden = true;
                columnInputElement.hidden = false;

                columns[i].style.backgroundColor = 'white';

                columnInputElement.focus();
            }   
        } );
    }

    rowElement.classList.toggle('hidden', false);
    rowElement.classList.toggle('summary-row-model', false);
}

function setColumnInputCapabilities(element){
    let inputs = Array.from(element.querySelectorAll('.summary-column textarea') );

    inputs.forEach(input => {
        input.addEventListener('focusout', () => {
            saveCellContent(input);
        } );
        input.addEventListener('keydown', (eventInfo) => {
            if(eventInfo.key !== 'Enter' && eventInfo.key !== 'Escape')
                return;
            if(input.hidden)
                return;
            saveCellContent(input);
        } )
    } );
}

function saveCellContent(input){
    let inputParent = input.parentElement;
    let contentElement = inputParent.querySelector('span');

    contentElement.innerText = input.value;
    contentElement.hidden = false;
    input.hidden = true;

    input.removeAttribute('style');
    inputParent.removeAttribute('style');
}

function onMouseMove(currentY){
    // Se o mouse não tiver sendo pressionado em nenhum elemento.
    if(targetMouseElement === undefined)
        return;

    // Obter a quantidade de pixels verticalmente que o mouse se moveu desde o início do movimento de mover um elemento.
    let y = currentY - targetMouseElement.initialY;

    // Obtemos todas as linhas, a altura do elemento que está sendo movido em pixels e a posição (index) do elemento que está sendo movido.
    let rows = Array.from(document.querySelectorAll('.summary-row') ).filter(row => row.classList.contains('header') === false).filter(row => row.classList.contains('hidden') === false);
    let height = targetMouseElement.element.getBoundingClientRect().height;
    let offset = parseInt(y / height);
    let index = rows.indexOf(targetMouseElement.element);

    //console.log(`y: ${y} | index: ${index} | height: ${height} | offset: ${offset}`);

    // Se alguma das seguintes condições for verdadeira, então é feito um retorno para ignorar o restante da função.
    if(y < 0 && index === 0 || y > 0 && index === rows.length - 1 || y === 0)
        return;
         
    // Movimento vertical relativo à posição inicial do elemento.
    targetMouseElement.element.style.top = y + 'px';
    
    // yCount recebe a quantidade de pixels movidos sempre positivamente.
    // E conforme a iteração do loop, é subtraído o tamanho do elemento que terá o lugar ocupado no valor de yCount.
    let targetIndexElement = null;
    let yCount = y < 0 ? y * -1 : y;

    let nextElement = null;

    for(let i = y < 0 ? index - 1 : index + 1; i >= 0 && i < rows.length; y > 0 ? i++ : i--){
        if(yCount < rows[i].getBoundingClientRect().height)
            break;
        
        nextElement = rows[i];
        yCount -= rows[i].getBoundingClientRect().height;
    }
    if(nextElement === null){
        targetMouseElement.targetElement = targetMouseElement.element;
        rows.forEach(row => { row.classList.toggle('to-replace', false); } );
        return;
    }

    targetMouseElement.targetElement = nextElement;
    rows.forEach(row => { row.classList.toggle('to-replace', false); } );
    nextElement.classList.toggle('to-replace', true);
}

function sortSummaryRows(index, toReplaceIndex){
    console.log(`sorting: ${index} in ${toReplaceIndex}.`);

    let container = document.querySelector('#summary-result');
    let rows = Array.from(document.querySelectorAll('.summary-row') );

    console.log('inicial:');
    rows.forEach(element => console.log(element) );
    
    if(index >= rows.length || toReplaceIndex >= rows.length || toReplaceIndex === index)
        return;
    if(index < 1 || toReplaceIndex < 1)
        return;

    let elements = [rows[index], rows[toReplaceIndex] ];
    
    // Caso seja adjacente, basta fazer a troca entre os dois.
    if(index === toReplaceIndex - 1 || index === toReplaceIndex + 1){
        rows[toReplaceIndex] = elements[0];
        rows[index] = elements[1];
    }
    else{
        rows.splice(index, 1);
        toReplaceIndex = rows.indexOf(elements[1] );
        // INDEX 2 é o primeiro elemento visual interativo. INDEX 0 é a linha do cabeçalho e o INDEX 1 é uma linha invísivel usada apenas para ser clonada na hora de inserir mais linhas.
        rows.splice(toReplaceIndex === 2 ? toReplaceIndex : toReplaceIndex + 1, 0, elements[0] );      
    }
        
    rows.forEach(element => {
        element.remove();
        container.appendChild(element);
    } );    
}

exportBtn.addEventListener('click', () => {
    let title = document.querySelector('#input-title').value;
    let fileName = '';

    while(fileName.trim().length === 0)
        fileName = prompt(decodeURI('Qual será o nome do arquivo?'), '');
    if(fileName === null)
        return;

    let rowElements = Array.from(document.getElementsByClassName('summary-row') ).filter(row => row.classList.contains('summary-row-model') === false && row.classList.contains('header') == false);
    let rows = [];

    exportBtn.disabled = true;

    rowElements.forEach(row => {
        let emissionDate = row.querySelector('.input-column-emission').parentElement.querySelector('span').innerText;
        let nf = row.querySelector('.input-column-nf').parentElement.querySelector('span').innerText;
        let client = row.querySelector('.input-column-client').parentElement.querySelector('span').innerText;
        let city = row.querySelector('.input-column-city').parentElement.querySelector('span').innerText;
        let volumes = row.querySelector('.input-column-volume').parentElement.querySelector('span').innerText;
        let weight = row.querySelector('.input-column-weight').parentElement.querySelector('span').innerText;
        let value = row.querySelector('.input-column-value').parentElement.querySelector('span').innerText;

        rows.push( {
          emissionDate: emissionDate,
          nf: nf,
          client: client,
          city: city,
          volumes: volumes,
          weight: volumes,
          weight: weight,
          value: value  
        } );
    } );

    let json = {
        outputFolder: exportBtn.getAttribute('output_folder'),
        fileName: fileName,
        title: title,
        rows: rows
    }

    let req = new XMLHttpRequest();
    req.open('POST', '/generateExcelFile');
    req.addEventListener('loadend', () => {
        exportBtn.disabled = false;
        alert(decodeURI('Solicitação de relatório enviada com sucesso!\nO sistema irá tentar criar e abrir o arquivo.') );
    } );
    req.send(JSON.stringify(json) );
} );
document.addEventListener('mousemove', (eventInfo) => { onMouseMove(eventInfo.screenY + window.scrollY) } );