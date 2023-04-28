<?php
require_once 'vendor/autoload.php';
require_once './library.php';
use thiagoalessio\TesseractOCR\TesseractOCR;


$jsonConfigFile = loadDefinition('http://192.168.1.115/ocr/DocDefinitions/company.json');
//echo json_last_error();

foreach ($jsonConfigFile as $key=>$value) {
  // echo $key;
  // var_dump($value);
}


$file = file_get_contents($_GET['url']);
file_put_contents('file.png', $file);

 $test = (new TesseractOCR('file.png'))
   // ->psm(6)
   // ->userWords('./config/userWords.txt')
    ->lang('eng','ces')
    ->tempDir('./tempDir')
 //   ->whitelist(range(0, 9), range('A', 'Z'))
	->batch('nochop')
	->makebox()
    ->hocr()
    ->run();

echo($test);

//foreach((new TesseractOCR())->availableLanguages() as $lang) echo $lang;


?>