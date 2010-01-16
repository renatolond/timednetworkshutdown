#!/usr/bin/perl

use strict;
use warnings;
use Proc::Daemon;
use Proc::PID::File;
use LWP::Simple;
use Sys::Hostname;

use constant LOG_DIR => '/var/log/timedNetworkShutdown';
use constant LOG_FILE => 'timedNetworkShutdown.log';
use constant PIDDIR => LOG_DIR;

use Log::Dispatch;
use Log::Dispatch::File;
use Date::Format;

eval { 
	Proc::Daemon::Init(); 
};
if ($@) {
	open my $fh, '>>', '/root/error'
		or warn "Can't open /root/error for writing: $!";
	print $fh "não foi possível iniciar o daemon\n";
	close $fh;
	die;
}
if (Proc::PID::File->running(name => "timedNetworkShutdown"))
  { 
	open my $fh, '>>', '/root/error'
		or warn "Can't open /root/error for writing: $!";
	print $fh "não foi possível iniciar o daemon\n";
	close $fh;
   warn "Already running!" }

my $hostname = hostname;
$hostname =~ s/(.*).dcc.ufrj.br/$1/g;

my $log = new Log::Dispatch(
		callbacks => sub { my %h=@_; return Date::Format::time2str('%d-%m-%Y %T', time)." ". $hostname ." $0\[$$]: ".$h{message}."\n"; }
		);
$log->add( Log::Dispatch::File->new( name      => 'file1',
			min_level => 'warning',
			mode      => 'append',
			filename  => File::Spec->catfile(LOG_DIR, LOG_FILE),
			)
	 );

$log->warning("daemon foi ligado em: ".time());
my $continue = 1;
my $basepagina = "http://www.dcc.ufrj.br/~lond/shutdown.php";

#$SIG{TERM} = 'interrupt';

sub interrupt {
	$log->warning("interrompido por ".@_.time());
	die;
}


sub pegaresposta
{
	my $conteudo = get $_[0];
	if ($@) {
		$log->error("Erro ao acessar ".$_[0]." em ".time());
		$log->error("Resposta: ".$@);
		return "nao";
	}
	return $conteudo;
}

sub perguntaservidor
{
	my $pagina = $basepagina . "?nome=" . $hostname;
	my $resp = pegaresposta($pagina);
	if ($resp =~ m/sim/i) {
		return 1;
	}
	elsif ($resp !~ m/nao/i) {
		$log->critical("String de resposta inesperada em ".time());
		$log->critical("Resposta: ".$resp);
	}

	return 0;
}

sub shutdownnow
{
	$log->critical("Desligando o computador em ".time());
	system(`shutdown -h now`);
}

while ($continue) {

	my $itstime = perguntaservidor();
	if ($itstime) {
		shutdownnow();
	}
#sleep for 30 minutes
	sleep(60*30);
}
